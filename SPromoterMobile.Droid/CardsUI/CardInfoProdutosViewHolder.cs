using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace spromotermobile.droid
{
	public class CardInfoProdutosViewHolder : RecyclerView.ViewHolder
	{
		internal readonly TextView type;
		internal readonly TextView description;
		internal readonly TextView btn1;
		internal readonly TextView btn2;

		public CardInfoProdutosViewHolder(View view) : base(view)
		{
			type = view.FindViewById<TextView>(Resource.Id.biggerTittle_produtos);
			description = view.FindViewById<TextView>(Resource.Id.mediumTittle_produtos);
			btn1 = view.FindViewById<TextView>(Resource.Id.firstDescription_produto);
			btn2 = view.FindViewById<TextView>(Resource.Id.secondDescription_produto);
		}
	}
}

