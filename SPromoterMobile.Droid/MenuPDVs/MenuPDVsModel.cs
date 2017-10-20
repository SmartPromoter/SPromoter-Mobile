using System.Collections.Generic;
using Android.Widget;
using Android.Support.V7.Widget;
using spromotermobile.droid.CardsUI;
using Refractored.Controls;

namespace spromotermobile.droid.MenuPDVs
{
    
    public class MenuPDVsModel : SPromoterMobile.MenuPdvsModel
    {
		
        public static readonly string ACTION_FINISHED_SYNC = "spromotermobile.droid.ACTION_FINISHED_SYNC";
        internal RelativeLayout msg;
        internal TextView userName;
        internal TextView txtMetaDiaria;
        internal ProgressBar barMetaDiaria;
        internal ProgressBar progressBar;
        internal RecyclerView cardList;
        internal MapMenuPDVsAdapter adapter;
        internal CircleImageView profileAvatar;
		internal GenericActivityModel modelGeneric;
		public Camera camera;
		public List<CardMenuPDVsModel> pdvs;

    }
}