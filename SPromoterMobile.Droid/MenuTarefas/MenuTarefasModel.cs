using System.Collections.Generic;
using Android.Widget;
using Refractored.Controls;

namespace spromotermobile.droid
{
    
    public class MenuTarefasModel : GenericActivityModel
	{
		internal CardInfoProdutosAdapter adapterProdutos;
		internal CardInfoLojaAdapter adapterLoja;
		internal List<CardInfoProdutoModel> list_produtos = new List<CardInfoProdutoModel>();
		internal List<CardInfoLojaModel> list_loja = new List<CardInfoLojaModel>();
		internal CircleImageView profileAvatar;
		public Camera camera;
		internal SPromoterMobile.MenuTarefasModel modelPCL;
		internal TextView txtMetaDiaria;
		internal ProgressBar barMetaDiaria;


	}
}

