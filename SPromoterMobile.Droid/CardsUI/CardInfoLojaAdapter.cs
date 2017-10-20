using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace spromotermobile.droid
{
	public class CardInfoLojaAdapter : RecyclerView.Adapter
	{
		List<CardInfoLojaModel> listCardsLoja;

		public override int ItemCount
		{
			get { return listCardsLoja == null ? 0 : listCardsLoja.Count; }
		}

		public void SetLojasOnList(List<CardInfoLojaModel> listCardsLoja)
		{
			this.listCardsLoja = listCardsLoja;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var viewHolder = (CardInfoLojaViewHolder)holder;
			var lojaItem = listCardsLoja[position];
			viewHolder.type.Text = lojaItem.type;
			viewHolder.description.Text = lojaItem.description;
			viewHolder.btn1.Text = lojaItem.btn1Desc;
			if (!viewHolder.btn1.HasOnClickListeners)
			{
				viewHolder.btn1.Click += lojaItem.btn1;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_card_loja, parent, false);
			return new CardInfoLojaViewHolder(view);
		}

		internal CardInfoLojaModel GetItemLoja(CardView cardView)
		{
			var result = new CardInfoLojaModel();
			var relativa = (RelativeLayout)cardView.GetChildAt(0);
			result.type = ((TextView)relativa.GetChildAt(0)).Text;
			result.description = ((TextView)relativa.GetChildAt(1)).Text;
			return result;
		}
	}
}

