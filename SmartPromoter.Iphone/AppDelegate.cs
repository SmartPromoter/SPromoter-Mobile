using System;
using System.Collections.Generic;
using AVFoundation;
using Foundation;
using SPromoterMobile;
using SPromoterMobile.Controller;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Exceptions;
using UIKit;
using UserNotifications;
#if !DEBUG
using HockeyApp.iOS;
using HockeyApp;
#endif


namespace SmartPromoter.Iphone
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {

        SyncronizerModel model;
        SyncronizerCon controller;
        bool autenticado;
        static bool itsRunning;

        public override UIWindow Window { get; set; }

        UIStoryboard MainStoryboard
        {
            get { return UIStoryboard.FromName("Main", NSBundle.MainBundle); }
        }

        UIViewController GetViewController(UIStoryboard storyboard, string viewControllerName)
        {
            return storyboard.InstantiateViewController(viewControllerName);
        }

        void PopulateSync()
        {
            if (model == null || controller == null)
            {
                model = new SyncronizerModel
                {
                    db = new SyncronizerDA(Sqlite_IOS.DB.dataBase),
                    dbCache = new CacheDA(Sqlite_IOS.DB.dataBase)
                };
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                controller = new SyncronizerCon(model);
            }
        }

        void SetRootViewController(UIViewController rootViewController, bool animate)
        {
            if (animate)
            {
                var transitionType = UIViewAnimationOptions.TransitionFlipFromRight;

                Window.RootViewController = rootViewController;
#pragma warning disable iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
                UIView.Transition(Window, 0.5, transitionType,
#pragma warning restore iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
                                  () =>
                                  {
                                      Window.RootViewController = rootViewController;
                                  }, null);
            }
            else
            {
                Window.RootViewController = rootViewController;
            }
        }

        public int ExecAPIs()
        {
            if (!itsRunning)
            {
                itsRunning = true;
                PopulateSync();
                try
                {
                    controller.ExecRestApis();
                }
                //O XGH só existe quando é encontrado. Se ta funcionando NAO RELA A MAO.
                catch (InvalidLoginException invalid)
                {
                    try
                    {
                        InvokeOnMainThread(delegate
                        {
                            model.db.RemoveUser(invalid.userID);
                        });
                    }
                    catch (InvalidOperationException)
                    {
                        return -99;
                    }
                }
#if !DEBUG
                catch (Exception ex)
                {
                    MetricsManager.TrackEvent("SyncDataFail");
                    MetricsManager.TrackEvent(ex.Message);
                }
#endif
                var NewPdvs = new List<string>();
                InvokeOnMainThread(delegate
                {
                    NewPdvs = controller.GetNovosPdvsNotification();
                    if (NewPdvs.Count > 0)
                    {
                        var content = new UNMutableNotificationContent
                        {
                            Title = "Novos PDVs",
                            Subtitle = "Roteiro Atualizado",
                            Body = "Existem " + NewPdvs.Count + " novos PDVs cadastrados",
                            Badge = NewPdvs.Count
                        };
                        var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

                        var requestID = "newPdvs";
                        var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

                        UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => { });
                    }
                });
                itsRunning = false;
                return NewPdvs.Count;
            }
            return 0;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
            try
            {
                var loginDA = new LoginDA(Sqlite_IOS.DB.dataBase);
                InvokeOnMainThread(delegate
                {
                    if (loginDA.GetInfoUsuario() == null)
                    {
                        autenticado = false;
                    }
                    else
                    {
                        autenticado = true;
                    }
                });
            }
            catch (Exception)
            {
                autenticado = false;
            }
            finally
            {
#if !DEBUG
                var manager = BITHockeyManager.SharedHockeyManager;
                manager.Configure("642401fb0ca946e5ab22ab2c69cb2cb2");
                manager.CrashManager.CrashManagerStatus = BITCrashManagerStatus.AutoSend;
                manager.StartManager();
#endif

                if (!autenticado)
                {
                    var loginViewController = GetViewController(MainStoryboard, "Login") as LoginController;
                    SetRootViewController(loginViewController, false);
                }
                else
                {
                    var modelPDV = new MenuPdvsModel
                    {
                        dbGenericActivity = new GenericActDA(Sqlite_IOS.DB.dataBase),
                        dbPdvs = new MenuPdvsDA(Sqlite_IOS.DB.dataBase)
                    };
                    InvokeOnMainThread(
                    delegate
                    {
                        modelPDV.infoUsuario = modelPDV.dbGenericActivity.GetUsersIDsLogged();
                        var visita = modelPDV.dbPdvs.GetVisitaAtual();
                        if (visita != null)
                        {
                            if (visita.Count > 0)
                            {
                                var TarefasViewController = GetViewController(MainStoryboard, "TarefasController") as TarefasController;
                                SetRootViewController(TarefasViewController, false);
                            }
                        }
                    });

#if !DEBUG
                    if (modelPDV != null && modelPDV.infoUsuario[0] != null)
                    {
                        manager.UserId = modelPDV.infoUsuario[0].ID;
                        manager.UserEmail = modelPDV.infoUsuario[0].SERVIDOR;
                        manager.StartManager();
                    }
#endif
                }
            }
            UNUserNotificationCenter.Current.GetNotificationSettings((settings) =>
            {
                if (settings.AlertSetting != UNNotificationSetting.Enabled)
                {
                    UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => { });
                }

                if (settings.BadgeSetting != UNNotificationSetting.Enabled)
                {
                    UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Badge, (approved, err) => { });
                }

                if (settings.SoundSetting != UNNotificationSetting.Enabled)
                {
                    UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Sound, (approved, err) => { });
                }
            });

            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            LocationHelper.UpdateLocation();
            return true;
        }

        public override void DidEnterBackground(UIApplication application)
        {
#if !DEBUG
            MetricsManager.TrackEvent("DidEnterBackground");
#endif
            Sincronizador.RunSincronizador(true);
            itsRunning = Sincronizador.itsRunning;
        }


        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            Sincronizador.RunSincronizador(true);
            itsRunning = Sincronizador.itsRunning;
            completionHandler(UIBackgroundFetchResult.NewData);
        }


        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
#if !DEBUG
            MetricsManager.TrackEvent("OnActivated");
#endif
            if (ExecAPIs() == -99)
            {
                var loginViewController = GetViewController(MainStoryboard, "Login") as LoginController;
                SetRootViewController(loginViewController, false);
            }
        }
    }
}


