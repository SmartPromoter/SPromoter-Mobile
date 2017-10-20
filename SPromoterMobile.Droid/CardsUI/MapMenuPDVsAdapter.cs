using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Java.Lang;

namespace spromotermobile.droid.CardsUI
{
    public class MapMenuPDVsAdapter : RecyclerView.Adapter, IFilterable
    {
        #region variaveis
        List<CardMenuPDVsModel> mMapLocations;
        List<CardMenuPDVsModel> mMapLocationsNoFiltered;

        public override int ItemCount
        {
            get
            {
                return mMapLocations == null ? 0 : mMapLocations.Count();
            }
        }

        public Filter Filter
        {
            get
            {
                mMapLocations = mMapLocationsNoFiltered;
                return new PDVFilter(this, mMapLocations);
            }
        }
        #endregion variaveis

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = (MapLocationViewHolder)holder;
            CardMenuPDVsModel mapLocation = mMapLocations[position];
            viewHolder.mediumTitle.Text = mapLocation.name;
            viewHolder.smallerTitle.Text = mapLocation.endereco;
            viewHolder.firstDescription.Text = mapLocation.btn1;
            if (!viewHolder.firstDescription.HasOnClickListeners)
            {
                viewHolder.firstDescription.Click += mapLocation.btnEventClick1;
            }
            viewHolder.secondDescription.Text = mapLocation.btn2;
            if (!viewHolder.secondDescription.HasOnClickListeners)
            {
                viewHolder.secondDescription.Click += (mapLocation.btnEventClick2);
            }
            viewHolder.map.Text = mapLocation.btn3;
            if (!viewHolder.map.HasOnClickListeners)
            {
                viewHolder.map.Click += (mapLocation.btnEventClick3);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_card_pdv, parent, false);
            return new MapLocationViewHolder(view);
        }

        internal void SetMapLocations(List<CardMenuPDVsModel> mapLocations)
        {
            mMapLocations = mapLocations;
            mMapLocationsNoFiltered = mapLocations;
        }


        internal CardMenuPDVsModel GetItemPDV(CardView cardView, List<CardMenuPDVsModel> listCard)
        {
            var result = new CardMenuPDVsModel();
            var relativa = (RelativeLayout)cardView.GetChildAt(0);
            result.name = ((TextView)relativa.GetChildAt(0)).Text;
            result.endereco = ((TextView)relativa.GetChildAt(1)).Text;
            foreach (CardMenuPDVsModel item in listCard)
            {
                if (item.name.Equals(result.name) && item.endereco.Equals(result.endereco))
                {
					result.latitude = item.latitude;
					result.longitude = item.longitude;

                    result.listTypePdv = item.listTypePdv;
                    break;
                }
            }
            return result;
        }

        class PDVFilter : Filter
        {
            readonly MapMenuPDVsAdapter adapter;
            readonly List<CardMenuPDVsModel> filteredList;
            List<CardMenuPDVsModel> originalList;

            public PDVFilter(MapMenuPDVsAdapter adapter, List<CardMenuPDVsModel> originalList)
            {
                this.adapter = adapter;
                this.originalList = originalList;
                filteredList = new List<CardMenuPDVsModel>();
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                filteredList.Clear();

                if (!constraint.Any())
                {
                    filteredList.AddRange(originalList);
                }
                else
                {
                    var filterPattern = constraint.ToString().ToLower().Trim();
                    foreach (CardMenuPDVsModel item in originalList)
                    {
                        if (item.endereco.ToLower().Contains(filterPattern) ||
                                item.name.ToLower().Contains(filterPattern))
                        {
                            filteredList.Add(item);
                        }
                    }
                }
                return null;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                adapter.mMapLocations = filteredList;
                adapter.NotifyDataSetChanged();
            }
        }
    }
}