//
//  Sincronizador.cs
//
//  Author:
//       leonardcolusso <leonardcolusso@gmail.com>
//
//  Copyright (c) 2017 SmartPromoter
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using HockeyApp;
using SPromoterMobile.Controller;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Exceptions;
using SPromoterMobile.Models.Tables;
using UIKit;
using UserNotifications;

namespace SmartPromoter.Iphone
{
    public static class Sincronizador
    {
        static SyncronizerModel model;
        static SyncronizerCon controller;
        static AzureStorage storage;
        public static bool itsRunning;
        static nint taskID;


        public static void RunSincronizador(bool isToUpdateImage)
        {
            if (!itsRunning)
            {
                taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
                try
                {
                    ExecAPIs(isToUpdateImage);
                }
                catch (Exception ex)
                {
#if !DEBUG
                    MetricsManager.TrackEvent("SyncDataFail");
                    MetricsManager.TrackEvent(ex.Message);
#endif
#if DEBUG
                    throw ex;
#endif
                }
                UIApplication.SharedApplication.EndBackgroundTask(taskID);

            }
        }

        static void ExecAPIs(bool isToUpdateImage)
        {
            itsRunning = true;
            PopulateSync();
            try
            {
                controller.ExecRestApis();
                if (isToUpdateImage)
                {
                    UploadImages();
                }
            }
            catch (InvalidLoginException invalid)
            {
                try
                {
                    model.db.RemoveUser(invalid.userID);
                }
                catch (InvalidOperationException)
                {
                    Process.GetCurrentProcess().CloseMainWindow();
                }
            }

            catch (Exception ex)
            {
#if !DEBUG
                MetricsManager.TrackEvent("SyncDataFail");
                MetricsManager.TrackEvent(ex.Message);
#endif
#if DEBUG
                throw ex;
#endif
            }
            var NewPdvs = new List<string>();
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
            itsRunning = false;
        }

        static void PopulateSync()
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

        static void UploadImages()
        {
            var container = new ContainerRestCon();
            var fotos = container.ListFotos();
            var infoUser = new List<TB_USUARIO>();
            infoUser = model.db.SelectInfoDeUsuarios();
            if (infoUser != null)
            {
                foreach (var item in infoUser)
                {
                    try
                    {
                        if (storage == null)
                        {
                            storage = new AzureStorage();
                        }
                        if (storage.clientAzureStorage == null)
                        {
                            storage = new AzureStorage();
                        }
                        foreach (var foto in fotos)
                        {
                            if (foto.Contains("VISITA"))
                            {
                                using (var img = container.LerArquivo(foto))
                                {
                                    if (img != null)
                                    {
                                        storage.UploadImage(img, foto, item.SERVIDOR);
                                        container.DeleteArquivo(foto);
                                    }
                                }
                            }
                        }
                        if (item.AVATAR_STATUS != (int)StatusAPI.CONCLUIDO && item.AVATAR != null)
                        {
                            using (var img = container.LerArquivo(item.AVATAR))
                            {
                                if (img != null)
                                {
                                    storage.UploadImage(img, item.AVATAR, item.SERVIDOR);
                                    item.AVATAR_STATUS = (int)StatusAPI.CONCLUIDO;
                                    model.db.AtualizaUserInfo(item);
                                }
                            }
                        }
#if !DEBUG
                        MetricsManager.TrackEvent("UploadImage");
#endif
                    }
                    catch (BadRequestException) { storage = null; }
                    catch (UnauthorizedException) { storage = null; }
                    catch (ArgumentNullException) { storage = null; }
                }
            }
        }
    }
}
