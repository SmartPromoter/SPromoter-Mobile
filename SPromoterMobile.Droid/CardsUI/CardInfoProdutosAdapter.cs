using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Linq;

namespace spromotermobile.droid
{
    public class CardInfoProdutosAdapter : RecyclerView.Adapter, IFilterable
    {
        #region variaveis
        List<CardInfoProdutoModel> listCardsProdutos;
        List<CardInfoProdutoModel> listCardsProdutosFiltered;
        public override int ItemCount
        {
            get { return listCardsProdutos == null ? 0 : listCardsProdutos.Count; }
        }

        public Filter Filter
        {
            get
            {
                listCardsProdutos = listCardsProdutosFiltered;
                return new PDVFilter(this, listCardsProdutos);
            }
        }
        #endregion variaveis

        internal void SetProdutosOnList(List<CardInfoProdutoModel> itemModel)
        {
            listCardsProdutos = itemModel;
            listCardsProdutosFiltered = itemModel;
        }

        internal CardInfoProdutoModel GetItemProduto(CardView cardView)
        {
            var result = new CardInfoProdutoModel();
            var relativa = (RelativeLayout)cardView.GetChildAt(0);
            result.type = ((TextView)relativa.GetChildAt(0)).Text;
            result.description = ((TextView)relativa.GetChildAt(1)).Text;
            return result;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_card_produto, parent, false);
            return new CardInfoProdutosViewHolder(view);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = (CardInfoProdutosViewHolder)holder;
            var produtoItem = listCardsProdutos[position];
            viewHolder.type.Text = produtoItem.type;
            viewHolder.description.Text = produtoItem.description;
            viewHolder.btn1.Text = produtoItem.btn1Desc;
            viewHolder.btn2.Text = produtoItem.btn2Desc;
            if (!viewHolder.btn1.HasOnClickListeners)
            {
                viewHolder.btn1.Click += produtoItem.btn1;
            }
            if (!viewHolder.btn2.HasOnClickListeners)
            {
                viewHolder.btn2.Click += produtoItem.btn2;
            }
        }

        class PDVFilter : Filter
        {
            readonly CardInfoProdutosAdapter adapter;

            readonly List<CardInfoProdutoModel> originalList;

            readonly List<CardInfoProdutoModel> filteredList;

            public PDVFilter(CardInfoProdutosAdapter adapter, List<CardInfoProdutoModel> originalList)
            {
                this.adapter = adapter;
                this.originalList = originalList;
                filteredList = new List<CardInfoProdutoModel>();
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
                    foreach (var item in originalList)
                    {
                        if (item.description.ToLower().Contains(filterPattern) ||
                            item.type.ToLower().Contains(filterPattern))
                        {
                            filteredList.Add(item);
                        }
                    }
                }
                return null;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                adapter.listCardsProdutos = filteredList;
                adapter.NotifyDataSetChanged();
            }
        }
    }
}

