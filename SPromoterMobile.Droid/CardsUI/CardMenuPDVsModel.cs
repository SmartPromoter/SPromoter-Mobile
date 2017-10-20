using System;
using System.Collections.Generic;
using SPromoterMobile;

namespace spromotermobile.droid.CardsUI
{
    public class CardMenuPDVsModel : IDisposable
    {
        internal string name;
		internal double latitude;
		internal double longitude;
        internal string endereco;
        internal string btn1;
        internal EventHandler btnEventClick1;
        internal string btn2;
        internal EventHandler btnEventClick2;
		internal string btn3;
		internal EventHandler btnEventClick3;
		internal List<ListTypePDV> listTypePdv;

        public CardMenuPDVsModel()  { }


        public CardMenuPDVsModel(string name, double lat, double lng, string endereco,
		                         string btn1, EventHandler btnEventClick1, string btn2, EventHandler btnEventClick2,
		                         string btn3, EventHandler btnEventClick3, ListTypePDV listTypePdv)
        {
            this.name = name;
            this.endereco = endereco;
            this.btn1 = btn1;
            this.btn2 = btn2;
			this.btn3 = btn3;
            this.btnEventClick1 = btnEventClick1;
            this.btnEventClick2 = btnEventClick2;
			this.btnEventClick3 = btnEventClick3;
			latitude = lat;
			longitude = lng;
			this.listTypePdv = new List<ListTypePDV>();
			this.listTypePdv.Add(listTypePdv);
        }

        #region IDisposable Support
        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                name = null;
                endereco = null;
                btn1 = null;
                btnEventClick1 = null;
                btn2 = null;
                btnEventClick2 = null;
                btn3 = null;
                btnEventClick3 = null;
                listTypePdv = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}