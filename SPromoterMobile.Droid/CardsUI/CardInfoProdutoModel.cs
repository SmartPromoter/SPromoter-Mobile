using System;

namespace spromotermobile.droid
{
	public class CardInfoProdutoModel
	{
		internal string description;
		internal string type;
		internal string btn1Desc;
		internal string btn2Desc;
		internal EventHandler btn1;
		internal EventHandler btn2;

		public CardInfoProdutoModel() {}


		public CardInfoProdutoModel(string description, string type, string btn1Desc,
		                       string btn2Desc, EventHandler btn1,
		                       EventHandler btn2)
		{
			this.description = description;
			this.type = type;
			this.btn1 = btn1;
			this.btn2 = btn2;
			this.btn1Desc = btn1Desc;
			this.btn2Desc = btn2Desc;
		}
	}
}

