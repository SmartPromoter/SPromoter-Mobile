using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace spromotermobile.droid
{
	public class CardInfoLojaViewHolder : RecyclerView.ViewHolder
	{
		internal readonly TextView type;
		internal readonly TextView description;
		internal readonly TextView btn1;

		public CardInfoLojaViewHolder(View view) : base(view)
		{
			type = view.FindViewById<TextView>(Resource.Id.biggerTittle_loja);
			description = view.FindViewById<TextView>(Resource.Id.mediumTittle_loja);
			btn1 = view.FindViewById<TextView>(Resource.Id.firstDescription_loja);
		}

	}
}

