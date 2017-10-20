using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace spromotermobile.droid.CardsUI
{
    public class MapLocationViewHolder : RecyclerView.ViewHolder
    {
        internal TextView mediumTitle;
        internal TextView smallerTitle;
        internal TextView firstDescription;
        internal TextView secondDescription;
		internal TextView map;
        public MapLocationViewHolder(View view) : base(view)
        {
            mediumTitle = view.FindViewById<TextView>(Resource.Id.mediumTittle);
            smallerTitle = view.FindViewById<TextView>(Resource.Id.smallerTittle);
            firstDescription = view.FindViewById<TextView>(Resource.Id.firstDescription);
            secondDescription = view.FindViewById<TextView>(Resource.Id.secondDescription);
			map = view.FindViewById<TextView>(Resource.Id.map);

        }
    }
}