//
//  Sincronizador.cs
//
//  Author:
//       leonardcolusso <leonardcolusso@gmail.com>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Net;
using SPromoterMobile.Controller;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Exceptions;
using HockeyApp;
using spromotermobile.droid.Data;
using spromotermobile.droid.sync;
using System.Threading;
using spromotermobile.droid.MenuPDVs;

namespace spromotermobile.droid
{
    public static class Sincronizador
    {
        public static SyncronizerModel model;
        public static SyncronizerCon controller;
        static AzureStorage storage;
        public static Context context;
        public static bool itsRunning;
        public static DateTime lastHitSync = DateTime.Now;


        public static void TryExecSync()
        {

            if (!CustomContentResolver.GetCustomContentResolver(context).IsRunning())
            {
#pragma warning disable RECS0002 // Convert anonymous method to method group
                new Thread(new ThreadStart(() => ExecSync())).Start();
#pragma warning restore RECS0002 // Convert anonymous method to method group
            }
        }

        public static void ExecSyncUI()
        {
            MetricsManager.TrackEvent("UIPerformSync");
            model = new SyncronizerModel
            {
                versionName = GetVersionName(),
                db = new SyncronizerDA(SQLite_Android.DB.dataBase),
                dbCache = new CacheDA(SQLite_Android.DB.dataBase)
            };
            controller = new SyncronizerCon(model);
            controller.ExecRestApis();
        }

        public static void ExecSync()
        {
            var difTime = (DateTime.Now - lastHitSync).TotalSeconds;
            if (difTime >= 11)
            {
                lastHitSync = DateTime.Now;
                itsRunning = true;
                MetricsManager.TrackEvent("PerformSync");
                model = new SyncronizerModel
                {
                    versionName = GetVersionName(),
                    db = new SyncronizerDA(SQLite_Android.DB.dataBase),
                    dbCache = new CacheDA(SQLite_Android.DB.dataBase)
                };
                controller = new SyncronizerCon(model);
                try
                {
                    UploadImages();
                    controller.ExecRestApis();
                }
                catch (InvalidLoginException invalid)
                {
                    try
                    {
                        model.db.RemoveUser(invalid.userID);
                    }
                    catch (InvalidOperationException) { }
                    finally
                    {
                        context.SendBroadcast(new Intent(MenuPDVsModel.ACTION_FINISHED_SYNC));
                    }
                }
                catch (Java.Lang.Exception)
                {
                    MetricsManager.TrackEvent("SyncDataFail");
                }
                finally
                {
                    context.SendBroadcast(new Intent(MenuPDVsModel.ACTION_FINISHED_SYNC));
                }

                itsRunning = false;
                if (string.IsNullOrEmpty(controller.exceptionMessage) || controller.exceptionMessage == "Socket closed")
                {
                    AddNotificationPdv(controller.GetNovosPdvsNotification());
                }
                else if (controller.invalidException != null)
                {
                    try
                    {
                        model.db.RemoveUser(controller.invalidException.userID);
                    }
                    catch (InvalidOperationException) { }
                    finally
                    {
                        context.SendBroadcast(new Intent(MenuPDVsModel.ACTION_FINISHED_SYNC));
                    }
                }
                else
                {
                    context.SendBroadcast(new Intent(MenuPDVsModel.ACTION_FINISHED_SYNC));
                    if (!controller.exceptionMessage.ToUpper().Contains("RESET BY PEER") ||
                        controller.exceptionMessage.ToUpper().Contains("SOCKET") ||
                        controller.exceptionMessage.ToUpper().Contains("TIMEOUT") ||
                        controller.exceptionMessage.ToUpper().Contains("TIME OUT"))
                    {
                        MetricsManager.TrackEvent("SyncDataFail");
                        MetricsManager.TrackEvent(controller.exceptionMessage);
#if DEBUG
						Console.WriteLine(controller.exceptionMessage);
						throw new Exception(controller.exceptionMessage);
#endif
                    }
                }
            }
        }
        static string GetVersionName()
        {
            var packageiNFO = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            var version = packageiNFO.VersionName;
            if (string.IsNullOrEmpty(version))
            {
                return "NULL_VERSION_CODE";
            }
            return version;
        }

        static void AddNotificationPdv(List<string> pdvs)
        {
            if (pdvs.Count > 0)
            {
                var intent = new Intent(context, typeof(MenuPdvs));
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, (int)TipoNotificacao.NovosPdvs, intent, PendingIntentFlags.OneShot);

                var builder = new Notification.Builder(context)
                    .SetContentIntent(pendingIntent)
                    .SetDefaults(NotificationDefaults.All)
                                              .SetSmallIcon(Resource.Drawable.logosmartpromoter);

                var inboxStyle = new Notification.InboxStyle();
                builder.SetContentTitle(context.Resources.GetString(Resource.String.novos_pdvs, pdvs.Count));
                if (pdvs.Count > 3)
                {
                    inboxStyle.AddLine(pdvs[0]);
                    inboxStyle.AddLine(pdvs[1]);
                    inboxStyle.AddLine(pdvs[2]);
                    inboxStyle.SetSummaryText(context.Resources.GetString(Resource.String.novos_pdvs, pdvs.Count - 3));
                }
                else
                {
                    foreach (var item in pdvs)
                    {
                        inboxStyle.AddLine(item);
                    }
                }
                builder.SetStyle(inboxStyle);

                Notification notification = builder.Build();

                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager.Notify((int)TipoNotificacao.NovosPdvs, notification);
            }

        }

        static void UploadImages()
        {
            var container = new ContainerRestCon();
            var fotos = container.ListFotos();
            var infoUser = model.db.SelectInfoDeUsuarios();
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
                                var asyncPhoto = new AsyncPhotoBlob(storage);
                                var listObjects = new Java.Lang.Object[] { foto, item.SERVIDOR, false };
                                asyncPhoto.Execute(listObjects);
                            }
                        }
                        if (item.AVATAR_STATUS != (int)StatusAPI.CONCLUIDO && item.AVATAR != null)
                        {
                            var asyncPhotoProfile = new AsyncPhotoBlob(storage);
                            var listObjects = new Java.Lang.Object[] { item.AVATAR, item.SERVIDOR, true };
                            asyncPhotoProfile.Execute(listObjects);
                            item.AVATAR_STATUS = (int)StatusAPI.CONCLUIDO;
                            model.db.AtualizaUserInfo(item);
                        }
                        MetricsManager.TrackEvent("UploadImage");
                    }
                    catch (BadRequestException) { storage = null; }
                    catch (UnauthorizedException) { storage = null; }
                    catch (ConnectException) { storage = null; }
                    catch (ArgumentNullException) { storage = null; }
                }
            }
        }
    }
    public class AsyncPhotoBlob : AsyncTask
    {
        AzureStorage storage;
        readonly ContainerRestCon container;
        string foto;
        string pasta;
        bool isAvatarProfile;
        bool hasError;

        public AsyncPhotoBlob(AzureStorage storage)
        {
            this.storage = storage;
            container = new ContainerRestCon();
        }

        protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
        {
            try
            {
                foto = (string)@params[0];
                pasta = (string)@params[1];
                isAvatarProfile = (bool)@params[2];
                using (var img = container.LerArquivo(foto))
                {
                    if (img != null)
                    {
                        storage.UploadImage(img, foto, pasta).Wait();
                    }
                }
            }
            catch (Exception) { hasError = true; }
            return true;
        }

        protected override void OnPostExecute(Java.Lang.Object result)
        {
            base.OnPostExecute(result);
            if (!hasError && !isAvatarProfile)
            {
                container.DeleteArquivo(foto);
            }
            hasError = false;
        }
    }

}
