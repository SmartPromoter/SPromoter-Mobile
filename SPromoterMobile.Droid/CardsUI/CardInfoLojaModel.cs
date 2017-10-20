using System;

namespace spromotermobile.droid
{
	public class CardInfoLojaModel
	{
		internal string description;
		internal string type;
		internal string btn1Desc;
		internal EventHandler btn1;

		public CardInfoLojaModel() { }


		public CardInfoLojaModel(string description, string type, string btn1Desc, EventHandler btn1)
		{
			this.description = description;
			this.type = type;
			this.btn1 = btn1;
			this.btn1Desc = btn1Desc;
		}
	}
}

