using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using spromotermobile.droid.CardsUI;
using SPromoterMobile.Data;
using SPromoterMobile.Models.Tables;
using Java.Interop;
using Refractored.Controls;
using spromotermobile.droid.MenuPDVs;
using SPromoterMobile;
using System.Net;
using HockeyApp;
using spromotermobile.droid.Data;
using Android.Util;
using System.Threading;

namespace spromotermobile.droid
{

    //KeyStroke GoogleMaps Code SHA1 1B:5B:4D:77:BF:C1:79:6A:F5:81:E7:22:5E:EB:04:09:66:8A:DF:C3
    [Activity(Label = "MenuPDVs", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class MenuPdvs : GenericActivity
    {
        static MenuPdvs Instance;
        MenuPdvsCon controller;
        public static MenuPDVsModel model;
        static string filterPDV = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            model = new MenuPDVsModel
            {
                modelGeneric = modelActivity
            };
            SetContentView(Resource.Layout.menu_pdvs);

            var layoutManager = new GridLayoutManager(this, 1, LinearLayoutManager.Vertical, false);
            model.camera = new Camera(ApplicationContext);
            model.msg = FindViewById<RelativeLayout>(Resource.Id.linearMessageListPDV);
            model.userName = FindViewById<TextView>(Resource.Id.txtUserName);
            model.txtMetaDiaria = FindViewById<TextView>(Resource.Id.txtMetaDiaria);
            model.cardList = FindViewById<RecyclerView>(Resource.Id.card_list_pdv);
            model.cardList.SetLayoutManager(layoutManager);
            model.cardList.HasFixedSize = false;
            model.profileAvatar = FindViewById<CircleImageView>(Resource.Id.profile_image_pdv);
            model.barMetaDiaria = FindViewById<ProgressBar>(Resource.Id.pBarMetaDiaria);
            model.progressBar = FindViewById<ProgressBar>(Resource.Id.progressPDV);
            model.pdvs = new List<CardMenuPDVsModel>();
            model.adapter = new MapMenuPDVsAdapter();
            model.dbPdvs = new MenuPdvsDA(SQLite_Android.DB.dataBase);
            model.infoUsuario = model.dbPdvs.GetUserInfoLogged();
            model.userName.Text = model.infoUsuario[0].NOME;
            model.cardList.SetAdapter(model.adapter);
            if (model.infoUsuario[0].AVATAR != null)
            {
                model.profileAvatar.SetImageBitmap(model.camera.GetBitMap(model.infoUsuario[0].AVATAR));
            }
            controller = new MenuPdvsCon(model);
            HockeyApp.Android.CrashManager.Register(this, "1a94f1b2048b497e815bd97f36cbc3e9", new HockeyCrashManagerSettings(model.infoUsuario[0].ID, model.infoUsuario[0].SERVIDOR));
            PopulateProgressBar();
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);
            try
            {
                if (resultCode == Android.App.Result.Ok && requestCode == (int)Camera.CameraCode.OnActivityResultCode)
                {
                    var ids = new List<string>();
                    foreach (var idLoggado in model.infoUsuario)
                    {
                        ids.Add(idLoggado.ID);
                    }
                    var urlsFotosSalvas = model.camera.PerformOnActivity(ids);
                    model.dbPdvs.InsertFotoProfile(urlsFotosSalvas, ids);
                    model.infoUsuario = model.dbPdvs.GetUserInfoLogged();
                    model.profileAvatar.SetImageBitmap(model.camera.GetBitMap(model.infoUsuario[0].AVATAR));
                    MetricsManager.TrackEvent("FotoSucesso");
                }
                if (resultCode != Android.App.Result.Canceled && resultCode != Android.App.Result.Ok &&
                    requestCode == (int)Camera.CameraCode.OnActivityResultCode)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                MetricsManager.TrackEvent("FotoFalha");
                MetricsManager.TrackEvent(ex.Message);
                RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.erro_msg_ao_tirar_foto), ToastLength.Long).Show());
            }
        }

        public override void OnEnterAnimationComplete()
        {
            base.OnEnterAnimationComplete();
            model.profileAvatar.ClearAnimation();

        }

        protected override void OnResume()
        {
            base.OnResume();
            Instance = this;
            var manager = (NotificationManager)GetSystemService(NotificationService);
            model.modelGeneric.gps = GPS.GetGPSTracker(this.ApplicationContext);
            modelActivity.gps.UpdateLocation();
            manager.Cancel((int)TipoNotificacao.NovosPdvs);
            model.infoUsuario = model.dbPdvs.GetUserInfoLogged();
            var visita = model.dbPdvs.GetVisitaAtual();
            if (visita != null && visita.Count > 0)
            {
                var listIdsPDVs = new List<ListTypePDV>();
                foreach (var item in visita)
                {
                    listIdsPDVs.Add(new ListTypePDV(item.ID));
                }
                var listIdsUsers = new List<string>();
                foreach (var itemUser in model.infoUsuario)
                {
                    listIdsUsers.Add(itemUser.ID);
                }

                var i = new Intent(this, typeof(MenuTarefas));
                i.PutExtra("lojaSelecionada", visita[0].ENDERECO);
                i.PutStringArrayListExtra("idUser", listIdsUsers);
                i.PutExtra("visitas", new ListTypePDV().ToIntentVar(listIdsPDVs));
                StartActivity(i);
                OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                                              Resource.Animation.abc_slide_out_bottom);
                Finish();
            }
            CreateMapList();
            if (controller.CheckOutVisita(model.pdvs.Count))
            {
                Sincronizador.itsRunning = false;
                Sincronizador.lastHitSync = new DateTime();
                Sincronizador.TryExecSync();
                CheckOutMessage();
            }
        }

        public List<CardMenuPDVsModel> PopulateList()
        {
            model.pdvs = new List<CardMenuPDVsModel>();
            var result = model.pdvs;
            foreach (TB_VISITA pdv in controller.VisitasPendentes())
            {
                string[] fullGeoPT = pdv.ENDERECO.Split('\n');
                string itemLoja = fullGeoPT[0].Split('-')[1].Trim();
                string itemEndereco = (fullGeoPT[1] + " " + fullGeoPT[2].Replace(" - CEP:", ", CEP:"));

                var index = model.pdvs.FindIndex(CardMenuPDVsModel => (CardMenuPDVsModel.name.Equals(itemLoja) &&
                                                                       CardMenuPDVsModel.endereco.Equals(itemEndereco)));

                var infoPdv = new ListTypePDV(pdv.ID);
                if (index >= 0 && result.Count > 0)
                {
                    if (result[index].listTypePdv.FindIndex(ListTypePDV => (ListTypePDV.IdVisita.Equals(infoPdv.IdVisita))) < 0)
                    {
                        result[index].listTypePdv.Add(infoPdv);
                    }
                }
                else
                {
                    var justificativaTextView = new TextView(this)
                    {
                        Text = Resources.GetString(Resource.String.justificativa_card)
                    };
                    justificativaTextView.Click += delegate { };

                    var checkIn = new TextView(this)
                    {
                        Text = Resources.GetString(Resource.String.check_in_card)
                    };
                    checkIn.Click += delegate { };
                    var itemCard = new CardMenuPDVsModel(itemLoja, pdv.LAT_PDV, pdv.LONG_PDV, itemEndereco,
                                                         Resources.GetString(Resource.String.justificar_card), HandleClickJustificativa,
                                                         Resources.GetString(Resource.String.check_in_card), HandleClickCheckIN,
                                                         Resources.GetString(Resource.String.map_card), HandleClickMap, infoPdv);
                    result.Add(itemCard);
                }
            }
            return result;
        }

        void CreateMapList()
        {
            model.pdvs = PopulateList();
            model.progressBar.Visibility = ViewStates.Visible;
            SetTextAndImageMessage(Resources.GetString(Resource.String.carregando_pdv));
            if (model.pdvs.Any())
            {
                model.msg.Visibility = ViewStates.Gone;
                model.cardList.Visibility = ViewStates.Visible;
            }
            else if (!model.pdvs.Any())
            {
                model.msg.Visibility = ViewStates.Visible;
                model.cardList.Visibility = ViewStates.Gone;
            }

            if (model.adapter == null)
            {
                model.adapter = new MapMenuPDVsAdapter();
            }
            model.adapter.SetMapLocations(model.pdvs);
            model.adapter.NotifyDataSetChanged();
        }

        void CheckIn(CardMenuPDVsModel item)
        {
            try
            {
                var isToOpenTarefas = false;
                CheckApp();
                var batery = GetBatteryLevel();
                var gpsLocation = GPS.lastLocation;
                var cordenadaEsperada = controller.GetCoordinates(item.listTypePdv);
                var gpsEsperado = new Android.Locations.Location("GpsEsperado")
                {
                    Latitude = cordenadaEsperada[0],
                    Longitude = cordenadaEsperada[1]
                };
                if (gpsEsperado == null)
                {
                    isToOpenTarefas = true;
                    controller.CheckIn(item.listTypePdv, 0, 0, batery);
                }
                else
                {
                    var distance = gpsEsperado.DistanceTo(gpsLocation);
                    if (distance > 500 && (int)cordenadaEsperada[0] > 0 && (int)cordenadaEsperada[1] > 0)
                    {
                        AlertDialog.Builder dialogBuilder;
                        dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
                        dialogBuilder.SetTitle(Resources.GetString(Resource.String.PDV_Distante));
                        dialogBuilder.SetMessage(Resources.GetString(Resource.String.PDV_Distante_descricao));
                        dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                            delegate
                            {
                                try
                                {
                                    controller.CheckIn(item.listTypePdv, gpsLocation.Latitude, gpsLocation.Longitude, batery);
                                    controller.RegistroDePontoEletronico();
                                    MetricsManager.TrackEvent("CheckInLoja");
                                    isToRunning = false;
                                    var i = new Intent(this, typeof(MenuTarefas));
                                    i.PutExtra("lojaSelecionada", item.name);
                                    i.PutStringArrayListExtra("idUser", controller.PrepareIdsUserToIntent());
                                    i.PutExtra("visitas", new ListTypePDV().ToIntentVar(item.listTypePdv));
                                    var options = ActivityOptions.MakeSceneTransitionAnimation(this,
                                    Pair.Create(FindViewById(Resource.Id.profile_image_pdv), "profileImage"),
                                    Pair.Create(FindViewById(Resource.Id.profileBarLayout), "profileBar"),
                                    Pair.Create(FindViewById(Resource.Id.toolbar), "toolbar"));
                                    StartActivity(i, options.ToBundle());
                                }
                                catch (NullReferenceException)
                                {
                                    Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
                                }
                                catch (Java.Lang.NullPointerException)
                                {
                                    Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
                                }
                            });
                        dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
                        {
                            MetricsManager.TrackEvent("CancelCheckIn");
                        });
                        model.modelGeneric.dialog = dialogBuilder.Create();
                        RunOnUiThread(() => model.modelGeneric.dialog.Show());
                    }
                    else
                    {
                        try
                        {
                            isToOpenTarefas = true;
                            controller.CheckIn(item.listTypePdv, gpsLocation.Latitude, gpsLocation.Longitude, batery);
                        }
                        catch (NullReferenceException ex)
                        {
                            MetricsManager.TrackEvent("CheckInFail");
                            MetricsManager.TrackEvent(ex.Message);
                            Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
                        }
                        catch (Java.Lang.NullPointerException exPointer)
                        {
                            MetricsManager.TrackEvent("CheckInFail");
                            MetricsManager.TrackEvent(exPointer.Message);
                            Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
                        }
                    }
                }
                if (isToOpenTarefas)
                {
                    controller.RegistroDePontoEletronico();
                    MetricsManager.TrackEvent("CheckInLoja");
                    var i = new Intent(this, typeof(MenuTarefas));
                    i.PutExtra("lojaSelecionada", item.name);
                    i.PutStringArrayListExtra("idUser", controller.PrepareIdsUserToIntent());
                    i.PutExtra("visitas", new ListTypePDV().ToIntentVar(item.listTypePdv));
                    var options = ActivityOptions.MakeSceneTransitionAnimation(this,
                    Pair.Create(FindViewById(Resource.Id.profile_image_pdv), "profileImage"),
                    Pair.Create(FindViewById(Resource.Id.profileBarLayout), "profileBar"),
                    Pair.Create(FindViewById(Resource.Id.toolbar), "toolbar"));
                    StartActivity(i, options.ToBundle());
                }
            }
            catch (NullReferenceException)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
            }
            catch (Java.Lang.NullPointerException)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.erro_checkin), ToastLength.Long).Show();
            }
        }

        void ExecMapByGPSCord(CardMenuPDVsModel item)
        {
            var gmmIntentUri = Android.Net.Uri.Parse(
                string.Format("http://maps.google.com/maps?daddr={0}", WebUtility.UrlEncode(item.endereco)));
            try
            {
                MetricsManager.TrackEvent("MapClick");
                var mapIntent = new Intent(Intent.ActionView, gmmIntentUri);
                StartActivity(mapIntent);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(this, Resource.String.maps_not_found, ToastLength.Long);
            }
        }

        void Justificativa(string justificativa, CardMenuPDVsModel item)
        {
            try
            {
                CheckApp();
                var gpsLocation = GPS.lastLocation;
                var batery = GetBatteryLevel();
                if (gpsLocation == null)
                {
                    controller.Justificativa(item.listTypePdv, justificativa, 0, 0, batery);
                }
                else
                {
                    controller.Justificativa(item.listTypePdv, justificativa,
                                                 gpsLocation.Latitude, gpsLocation.Longitude, batery);
                }
                RemoveItem(item);
                controller.RegistroDePontoEletronico();
                if (controller.CheckOutVisita(model.pdvs.Count))
                {
                    CheckOutMessage();
                }
                PopulateProgressBar();
                MetricsManager.TrackEvent("JustificativaLoja");
                Toast.MakeText(this, Resources.GetString(Resource.String.justificativa_sucesso), ToastLength.Long).Show();

            }
            catch (NullReferenceException)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.erro_justificativa), ToastLength.Long).Show();
            }
        }

        void RemoveItem(CardMenuPDVsModel item)
        {
            var index = model.pdvs.FindIndex(CardMenuPDVsModel => (CardMenuPDVsModel.name.Equals(item.name) &&
               CardMenuPDVsModel.endereco.Equals(item.endereco)));
            if (index > -1)
            {
                model.pdvs.RemoveAt(index);
                if (model.adapter != null)
                {
                    model.adapter.NotifyDataSetChanged();
                }
            }
            if (string.IsNullOrEmpty(filterPDV) && model.adapter != null)
            {
                model.adapter.Filter.InvokeFilter("");
            }
            else if (model.adapter != null)
            {
                model.adapter.Filter.InvokeFilter(filterPDV);
            }
        }

        #region overrides-estaticos
        protected override void OnPause()
        {
            base.OnPause();
            GPS.GetGPSTracker(this.ApplicationContext).Remove();
            filterPDV = "";
        }


        public override void OnBackPressed()
        {
            FecharActivity();
        }

        protected override void OnDestroy()
        {
            Instance = null;
            base.OnDestroy();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            model.modelGeneric.myToolbar.SetNavigationIcon(Resource.Drawable.logo_mobile);
            model.modelGeneric.myToolbar.Title = Resources.GetString(Resource.String.menupdvtoolbar);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_almoco:
                    OnClickAlmoco();
                    break;
                case Resource.Id.action_search:
                    OnClickBtnPesquisa();
                    break;
                case Resource.Id.action_newUser:
                    OnClickBtnNewUser();
                    break;
                case Resource.Id.force_sync:
                    OnClickSync();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion overrides-estaricos

        #region handleClicks
        [Export("OnClickbtnPhotoProfile")]
        public void OnClickbtnPhotoProfile(View view)
        {
            StartActivityForResult(model.camera.PerfomCamera(), (int)Camera.CameraCode.OnActivityResultCode);
            OverridePendingTransition(Resource.Animation.abc_slide_in_top,
                                          Resource.Animation.abc_slide_out_top);
        }

        void OnClickBtnPesquisa()
        {
            var dialogView = LayoutInflater.Inflate(Resource.Layout.item_popup, null);
            dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup).Hint = Resources.GetString(Resource.String.desc);
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.searchpdv));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.search),
                delegate
                {
                    var txtPesquisa = dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup);
                    if (string.IsNullOrWhiteSpace(txtPesquisa.Text))
                    {
                        model.adapter.Filter.InvokeFilter("");
                    }
                    else
                    {
                        model.adapter.Filter.InvokeFilter(txtPesquisa.Text);
                    }
                    filterPDV = txtPesquisa.Text;
                });
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.cancelar), delegate
            {
                model.adapter.Filter.InvokeFilter("");
                filterPDV = "";
                MetricsManager.TrackEvent("CancelSearchPdvs");
            });
            if (!filterPDV.Equals(""))
            {
                dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup).Text = filterPDV;
            }
            dialogBuilder.SetView(dialogView);
            model.modelGeneric.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.modelGeneric.dialog.Show());
        }

        void HandleClickCheckIN(object sender, EventArgs e)
        {
            var item = model.adapter.GetItemPDV((CardView)((View)sender).Parent.Parent.Parent, model.pdvs);
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.check_in));
            dialogBuilder.SetMessage(Resources.GetString(Resource.String.chk_in_confirmacao));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                delegate { CheckIn(item); });
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
            {
                MetricsManager.TrackEvent("CancelCheckIn");
            });
            model.modelGeneric.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.modelGeneric.dialog.Show());
        }

        void HandleClickJustificativa(object sender, EventArgs e)
        {

            var spinnerArray = new List<string>
            {
                "Selecione...",
                "Ação em outro pdv por tempo integral",
                "Atestado Medico",
                "Bateria insuficiente",
                "Cumpri meu horário de trabalho",
                "Em treinamento",
                "Estava em reunião",
                "Loja fora do roteiro planejado",
                "PDV fechado",
                "PDV não existe",
                "Transito durante o transporte",
                "Outros"
            };
            var dialogView = LayoutInflater.Inflate(Resource.Layout.item_popup_dropdownlist, null);
            var spinnerArrayAdapter = new ArrayAdapter(this, Resource.Layout.support_simple_spinner_dropdown_item, spinnerArray);
            var spinner = dialogView.FindViewById<Spinner>(Resource.Id.edit_popup_dropdown);
            spinner.SetBackgroundResource(Resource.Drawable.spinner_background);
            spinner.Adapter = spinnerArrayAdapter;

            var item = model.adapter.GetItemPDV((CardView)((View)sender).Parent.Parent.Parent, model.pdvs);
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.justificativa));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.justificar),
                delegate
                {
                    var itemJustificativa = dialogView.FindViewById<Spinner>(Resource.Id.edit_popup_dropdown);
                    if (itemJustificativa.SelectedItemPosition > 0)
                    {
                        Justificativa(spinnerArray[itemJustificativa.SelectedItemPosition], item);
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.spinner_justificativa), ToastLength.Long).Show());
                    }
                });
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.cancelar), delegate
            {
                MetricsManager.TrackEvent("CancelJustificativa");
            });
            dialogBuilder.SetView(dialogView);
            model.modelGeneric.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.modelGeneric.dialog.Show());
        }

        void HandleClickMap(object sender, EventArgs e)
        {
            var item = model.adapter.GetItemPDV((CardView)((TextView)sender).Parent.Parent.Parent, model.pdvs);
            ExecMapByGPSCord(item);
        }
        #endregion handleClicks

        void CheckOutMessage()
        {
            model.msg.Visibility = ViewStates.Visible;
            model.progressBar.Visibility = ViewStates.Gone;
            model.cardList.Visibility = ViewStates.Gone;
            SetTextAndImageMessage(Resources.GetString(Resource.String.sem_pdv));
        }

        void OnClickSync()
        {
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.sincronizar));
            dialogBuilder.SetMessage(Resources.GetString(Resource.String.aguarde));
            dialogBuilder.SetCancelable(false);
            model.modelGeneric.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.modelGeneric.dialog.Show());
            new Thread(() =>
            {
                try
                {
                    Sincronizador.ExecSyncUI();
                    if (!string.IsNullOrEmpty(Sincronizador.controller.exceptionMessage))
                    {
                        throw new Exception(Sincronizador.controller.exceptionMessage);
                    }
                    RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.sync_ok), ToastLength.Long).Show());
                    SendBroadcast(new Intent(MenuPDVsModel.ACTION_FINISHED_SYNC));
                    RunOnUiThread(() => model.modelGeneric.dialog.Dismiss());
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => model.modelGeneric.dialog.Dismiss());
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            ErrorSync(ex.InnerException.InnerException);
                        }
                        else
                        {
                            ErrorSync(ex.InnerException);
                        }
                    }
                    else
                    {
                        ErrorSync(ex);
                    }
                }
            }).Start();

        }
        void ErrorSync(Exception ex)
        {
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.errosync));
            dialogBuilder.SetMessage(ex.Message);
            RunOnUiThread(() =>
            {
                model.modelGeneric.dialog = dialogBuilder.Create();
                dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.ok), delegate { });
                model.modelGeneric.dialog.Show();
            });
        }

        void SetTextAndImageMessage(string textMessage)
        {
            var txtViewMessagePDV = FindViewById<TextView>(Resource.Id.txtViewMessagePDV);

            txtViewMessagePDV.Text = textMessage;

        }

        void PopulateProgressBar()
        {
            var total = controller.PercentualVisitas();
            if (total == 0 || total == 999)
            {
                model.barMetaDiaria.Progress = 0;
                model.txtMetaDiaria.Text = Resources.GetString(Resource.String.percent_dinamico_bar, "0% ");
            }
            else
            {
                model.barMetaDiaria.Progress = total;
                model.txtMetaDiaria.Text = Resources.GetString(Resource.String.percent_dinamico_bar, total + "% ");
            }
        }

        #region broadcast
        [BroadcastReceiver(Label = "spromotermobile.droid.ACTION_FINISHED_SYNC")]
        [IntentFilter(new string[] { "spromotermobile.droid.ACTION_FINISHED_SYNC" })]
        class syncBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {

                if (intent.Action == "spromotermobile.droid.ACTION_FINISHED_SYNC" && Instance != null)
                {
                    Instance.CreateMapList();
                    Instance.PopulateProgressBar();
                    if (model.pdvs.Count <= 0)
                    {
                        model.msg.Visibility = ViewStates.Visible;
                        model.progressBar.Visibility = ViewStates.Gone;
                        model.cardList.Visibility = ViewStates.Gone;
                        Instance.SetTextAndImageMessage(Instance.Resources.GetString(Resource.String.sem_pdv));
                    }
                }
                return;
            }
        };
        #endregion broadcast


    }
}