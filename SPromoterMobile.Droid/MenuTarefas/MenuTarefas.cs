using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using HockeyApp;
using SPromoterMobile;
using spromotermobile.droid.Data;

namespace spromotermobile.droid
{
    [Activity(Label = "MenuTarefas", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class MenuTarefas : GenericActivity
    {
        static MenuTarefasModel model;
        static MenuTarefasCon controller;
        static string filterProdutos = "";
        static RecyclerView mRecyclerViewProdutos;
        static RecyclerView mRecyclerViewLoja;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            model = new MenuTarefasModel();
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.menu_tarefas);
            model.camera = new Camera(ApplicationContext);
            model.myToolbar = modelActivity.myToolbar;
            model.gps = modelActivity.gps;
            model.dialog = modelActivity.dialog;
            model.dbGenericActivity = modelActivity.dbGenericActivity;
            Window.EnterTransition = null;
            Window.SharedElementReenterTransition = null;
            Window.ReenterTransition = null;
            model.txtMetaDiaria = FindViewById<TextView>(Resource.Id.txtMetaDiariaTarefa);
            model.barMetaDiaria = FindViewById<ProgressBar>(Resource.Id.pBarMetaTarefa);

            model.modelPCL = new SPromoterMobile.MenuTarefasModel
            {
                idVisitas = new ListTypePDV().FromIntentVar(Intent.GetStringExtra("visitas")),
                idsUsuariosLogados = new List<string>(),
                db = new MenuTarefasDA(SQLite_Android.DB.dataBase)
            };
            foreach (var itemIds in Intent.GetStringArrayListExtra("idUser"))
            {
                model.modelPCL.idsUsuariosLogados.Add(itemIds);
            }

            model.profileAvatar = FindViewById<CircleImageView>(Resource.Id.profile_image_tarefa);
            var users = model.dbGenericActivity.GetUsersIDsLogged();
            var profile = users.Find((obj) => !string.IsNullOrEmpty(obj.AVATAR));
            if (profile != null && profile.AVATAR != null)
            {
                model.profileAvatar.SetImageBitmap(model.camera.GetBitMap(profile.AVATAR));
            }

            model.modelPCL.db = new MenuTarefasDA(SQLite_Android.DB.dataBase);
            model.adapterProdutos = new CardInfoProdutosAdapter()
            {
                HasStableIds = true
            };
            model.adapterLoja = new CardInfoLojaAdapter()
            {
                HasStableIds = true
            };
            var modelFormDinamico = new FormDinamicoModel
            {
                Db = new FormDinamicoDA(SQLite_Android.DB.dataBase)
            };
            model.modelPCL.formDinamico = new FormDinamicoCon(modelFormDinamico);
            controller = new MenuTarefasCon(model.modelPCL);
            var layoutManagerProduto = new GridLayoutManager(this, 1, LinearLayoutManager.Vertical, false);
            mRecyclerViewProdutos = FindViewById<RecyclerView>(Resource.Id.card_list_produtos);
            mRecyclerViewProdutos.SetLayoutManager(layoutManagerProduto);
            mRecyclerViewProdutos.HasFixedSize = true;
            mRecyclerViewProdutos.SetItemViewCacheSize(20);
            mRecyclerViewProdutos.DrawingCacheEnabled = true;
            mRecyclerViewProdutos.DrawingCacheQuality = DrawingCacheQuality.High;

            var layoutManagerLoja = new GridLayoutManager(this, 1, LinearLayoutManager.Vertical, false);
            mRecyclerViewLoja = FindViewById<RecyclerView>(Resource.Id.card_list_loja);
            mRecyclerViewLoja.SetLayoutManager(layoutManagerLoja);
            mRecyclerViewLoja.HasFixedSize = true;
            mRecyclerViewLoja.SetItemViewCacheSize(20);
            mRecyclerViewLoja.DrawingCacheEnabled = true;
            mRecyclerViewLoja.DrawingCacheQuality = DrawingCacheQuality.High;
        }

        protected override void OnResume()
        {
            isToRunning = true;
            base.OnResume();
            if (modelActivity != null)
            {
                model.myToolbar = modelActivity.myToolbar;
                model.gps = modelActivity.gps;
                model.dialog = modelActivity.dialog;
                model.dbGenericActivity = modelActivity.dbGenericActivity;
            }
            modelActivity.gps.UpdateLocation();

            if (controller.HasVisitaAntigaAtiva())
            {
                controller.CheckOutTarefa();
                model = null;
                controller = null;
                var i = new Intent(this, typeof(MenuPdvs));
                var options = ActivityOptions.MakeSceneTransitionAnimation(this,
                Pair.Create(FindViewById(Resource.Id.profile_image_tarefa), "profileImage"),
                Pair.Create(FindViewById(Resource.Id.profileLayout), "profileBar"),
                Pair.Create(FindViewById(Resource.Id.toolbar), "toolbar"));
                StartActivity(i, options.ToBundle());
            }
            else
            {
                RunOnUiThread(() =>
                {
                    PopulateProgressBar();
                    PopulateTarefas();
                    CheckOutTarefas();
                });
            }
        }


        public override void OnBackPressed()
        {
            FecharActivity();
        }

        protected override void OnPause()
        {
            base.OnPause();
            GPS.GetGPSTracker(ApplicationContext).Remove();
            filterProdutos = "";
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_tarefas, menu);
            model.myToolbar.SetNavigationIcon(Resource.Drawable.logo_mobile);
            model.myToolbar.Title = GetString(Resource.String.tarefas);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_almoco_tarefas:
                    OnClickAlmoco();
                    break;
                case Resource.Id.concluir_tarefa:
                    ConcluirTarefas();
                    break;
                case Resource.Id.action_search_tarefas:
                    OnClickBtnPesquisa();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        void PopulateTarefas()
        {
            model.list_produtos = new List<CardInfoProdutoModel>();
            model.list_loja = new List<CardInfoLojaModel>();
            foreach (var item in model.modelPCL.idVisitas)
            {
                var resultProduto = new List<CardInfoProdutoModel>();
                var resultLoja = new List<CardInfoLojaModel>();
                var listProdutos = controller.ListProdutos(item.IdVisita);
                if (listProdutos != null && listProdutos.Count > 0)
                {
                    foreach (var produto in listProdutos)
                    {
                        if (produto.ID != "00000000-0000-0000-0000-000000000000")
                        {
                            resultProduto.Add(new CardInfoProdutoModel(produto.NOME, produto.CATEGORIA,
                                            GetString(Resource.String.ruptura_card),
                                            GetString(Resource.String.formulario_card),
                                            HandleClickRuptura, HandleClickFormularioProduto));
                        }
                    }
                    model.list_produtos.AddRange(resultProduto);
                    if (model.modelPCL.db.HasTarefaLoja(item.IdVisita))
                    {
                        var logged = model.modelPCL.db.GetUsersIDsLogged();
                        var currentUser = logged.Find((obj) => obj.ID.Equals(
                            model.modelPCL.db.GetIDByVisita(item.IdVisita)));

                        var pdvInfo = model.modelPCL.db.GetLojaInfo(item.IdVisita);
                        resultLoja.Add(new CardInfoLojaModel(pdvInfo.ENDERECO, currentUser.SERVIDOR.Substring(0, currentUser.SERVIDOR.IndexOf(".", StringComparison.CurrentCulture)),
                                                                GetString(Resource.String.formulario_card), HandleClickFormularioLoja));
                    }
                    model.list_loja.AddRange(resultLoja);
                }
            }
            model.adapterLoja.SetLojasOnList(model.list_loja);
            model.adapterProdutos.SetProdutosOnList(model.list_produtos);
            mRecyclerViewLoja.SetAdapter(model.adapterLoja);
            mRecyclerViewProdutos.SetAdapter(model.adapterProdutos);
        }

        void RemoveItem(CardInfoProdutoModel item)
        {
            var index = model.list_produtos.FindIndex(CardInfoProdutoModel => (
                CardInfoProdutoModel.description.Equals(item.description) &&
                CardInfoProdutoModel.type.Equals(item.type)));
            if (index > -1)
            {
                model.list_produtos.RemoveAt(index);
                model.adapterProdutos.NotifyItemRemoved(index);
            }
            if (string.IsNullOrEmpty(filterProdutos))
            {
                model.adapterProdutos.Filter.InvokeFilter("");
            }
            else
            {
                model.adapterProdutos.Filter.InvokeFilter(filterProdutos);
            }
            PopulateProgressBar();
        }

        void RemoveItem(CardInfoLojaModel item)
        {
            var index = model.list_loja.FindIndex(CardInfoLojaModel => (
                CardInfoLojaModel.description.Equals(item.description) &&
                CardInfoLojaModel.type.Equals(item.type)));
            if (index > -1)
            {
                model.list_loja.RemoveAt(index);
                model.adapterLoja.NotifyItemRemoved(index);
            }
            PopulateProgressBar();
        }

        void CheckOutTarefas()
        {
            if (model.list_produtos.Count < 1 && model.list_loja.Count < 1)
            {
                controller.CheckOutTarefaLista();
                AlertDialog.Builder dialogBuilder;
                dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
                dialogBuilder.SetTitle(Resources.GetString(Resource.String.tarefas));
                dialogBuilder.SetMessage(Resources.GetString(Resource.String.dialog_confirm_sem_tarefas_no_pdv));
                dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.ok),
                    delegate
                    {
                        model = null;
                        controller = null;
                        var i = new Intent(this, typeof(MenuPdvs));
                        var options = ActivityOptions.MakeSceneTransitionAnimation(this,
                        Pair.Create(FindViewById(Resource.Id.profile_image_tarefa), "profileImage"),
                        Pair.Create(FindViewById(Resource.Id.profileLayout), "profileBar"),
                        Pair.Create(FindViewById(Resource.Id.toolbar), "toolbar"));
                        StartActivity(i, options.ToBundle());
                    }
                );
                dialogBuilder.SetCancelable(false);
                model.dialog = dialogBuilder.Create();
                RunOnUiThread(() => model.dialog.Show());
            }
        }

        void PopulateProgressBar()
        {
            int total = controller.PercentualTarefas();
            if (total == 0 || total == 999)
            {
                model.barMetaDiaria.Progress = 0;
                model.txtMetaDiaria.Text = Resources.GetString(Resource.String.percent_dinamico_bar_tarefa, "0% ");
            }
            else
            {
                model.barMetaDiaria.Progress = total;
                model.txtMetaDiaria.Text = Resources.GetString(Resource.String.percent_dinamico_bar_tarefa, total + "% ");
            }
        }


        #region handleClicks
        void OnClickBtnPesquisa()
        {
            var dialogView = LayoutInflater.Inflate(Resource.Layout.item_popup, null);
            dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup).Hint = Resources.GetString(Resource.String.desc);
            AlertDialog.Builder dialogBuilder;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            }
            else
            {
                dialogBuilder = new AlertDialog.Builder(this);
            }
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.search_produtos));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.search),
                delegate
                {
                    var txtPesquisa = dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup);
                    if (string.IsNullOrWhiteSpace(txtPesquisa.Text))
                    {
                        model.adapterProdutos.Filter.InvokeFilter("");
                    }
                    else
                    {
                        model.adapterProdutos.Filter.InvokeFilter(txtPesquisa.Text);
                    }
                    filterProdutos = txtPesquisa.Text;
                });
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.cancelar),
                delegate
                {
                    model.adapterProdutos.Filter.InvokeFilter("");
                    filterProdutos = "";
                    MetricsManager.TrackEvent("ClearSearchTarefas");
                });
            if (!filterProdutos.Equals(""))
            {
                dialogView.FindViewById<AutoCompleteTextView>(Resource.Id.edit_popup).Text = filterProdutos;
            }
            dialogBuilder.SetView(dialogView);
            model.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.dialog.Show());
        }

        void ConcluirTarefas()
        {
            MetricsManager.TrackEvent("ForceCheckOut");
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.tarefas));
            dialogBuilder.SetMessage(Resources.GetString(Resource.String.confirm_tarefas));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                delegate
                {
                    controller.CheckOutTarefa();
                    model = null;
                    controller = null;
                    var i = new Intent(this, typeof(MenuPdvs));
                    var options = ActivityOptions.MakeSceneTransitionAnimation(this,
                    Pair.Create(FindViewById(Resource.Id.profile_image_tarefa), "profileImage"),
                    Pair.Create(FindViewById(Resource.Id.profileLayout), "profileBar"),
                    Pair.Create(FindViewById(Resource.Id.toolbar), "toolbar"));
                    StartActivity(i, options.ToBundle());
                }
            );
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
            {
                MetricsManager.TrackEvent("CancelForceCheckOut");
            });
            model.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.dialog.Show());
        }

        void HandleClickRuptura(object sender, EventArgs e)
        {
            var item = model.adapterProdutos.GetItemProduto((CardView)((View)sender).Parent.Parent.Parent);
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.ruptura_card));
            dialogBuilder.SetMessage(Resources.GetString(Resource.String.ruptura_confirmacao));
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                delegate { SetRuptura(item); });
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
            {
                MetricsManager.TrackEvent("CancelRuptura");
            });
            model.dialog = dialogBuilder.Create();
            RunOnUiThread(() => model.dialog.Show());
        }

        void HandleClickFormularioLoja(object sender, EventArgs e)
        {
            var item = model.adapterLoja.GetItemLoja(
                (CardView)((View)sender).Parent.Parent.Parent);
            var userIdsLogados = model.dbGenericActivity.GetUsersIDsLogged();
            var idDoUserSelecionado = userIdsLogados.FindAll((obj) => obj.SERVIDOR.Contains(item.type));
            foreach (var itemIDVisita in model.modelPCL.idVisitas)
            {
                var idUser = controller.GetIDByVisita(itemIDVisita.IdVisita);
                foreach (var itemIdUser in idDoUserSelecionado)
                {
                    if (idUser.Equals(itemIdUser.ID))
                    {
                        var i = new Intent(this, typeof(FormDinamico));
                        i.PutExtra("idVisita", itemIDVisita.IdVisita);
                        i.PutExtra("idProduto", "00000000-0000-0000-0000-000000000000");
                        isToRunning = false;
                        StartActivity(i);
                        OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                                                      Resource.Animation.abc_slide_out_bottom);
                        break;
                    }
                }
            }
        }
        void HandleClickFormularioProduto(object sender, EventArgs e)
        {
            var item = model.adapterProdutos.GetItemProduto(
                (CardView)((View)sender).Parent.Parent.Parent);
            foreach (var itemIDVisita in model.modelPCL.idVisitas)
            {
                var tbProduto = controller.GetProdutosList(item.description);
                foreach (var produto in tbProduto)
                {
                    if (controller.IsCorrectIDTarefaLoja(itemIDVisita.IdVisita, produto.ID))
                    {
                        var i = new Intent(this, typeof(FormDinamico));
                        i.PutExtra("idUser", controller.GetIDByVisita(itemIDVisita.IdVisita));
                        i.PutExtra("idVisita", itemIDVisita.IdVisita);
                        if (tbProduto != null)
                        {
                            i.PutExtra("idProduto", produto.ID);
                        }
                        isToRunning = false;
                        StartActivity(i);
                        OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                                  Resource.Animation.abc_slide_out_top);
                        break;
                    }
                }
            }
        }

        void SetRuptura(CardInfoProdutoModel item)
        {
            try
            {
                CheckApp();
                var location = GPS.lastLocation;

                foreach (var itemIDVisita in model.modelPCL.idVisitas)
                {
                    var tbProduto = controller.GetProdutosList(item.description);
                    foreach (var produto in tbProduto)
                    {
                        if (controller.IsCorrectIDTarefaLoja(itemIDVisita.IdVisita, produto.ID))
                        {
                            var batery = GetBatteryLevel();
                            if (location == null)
                            {
                                model.modelPCL.formDinamico.SetRuptura(controller.GetProdutos(item.description).ID,
                                    itemIDVisita.IdVisita, 0, 0, batery);
                            }
                            else
                            {
                                model.modelPCL.formDinamico.SetRuptura(controller.GetProdutos(item.description).ID,
                                        itemIDVisita.IdVisita, location.Latitude, location.Longitude, batery);
                            }
                        }
                    }
                }
                RemoveItem(item);
                CheckOutTarefas();
                RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.ruptura_informada_sucesso), ToastLength.Long).Show());
                MetricsManager.TrackEvent("Ruptura");
            }
            catch (NullReferenceException ex)
            {
                MetricsManager.TrackEvent("RupturaFail");
                MetricsManager.TrackEvent(ex.Message);
                RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.erro_ruptura), ToastLength.Long).Show());
            }
        }
        #endregion handleclicks
    }
}

