using System;
using System.Collections.Generic;
using SPromoterMobile;

namespace SmartPromoter.Iphone
{
    public class Pdv
    {
        public string IdVisita { get; set; }
        public string Endereco { get; set; }
        public string NomePDV { get; set; }
        public double Longi { get; set; }
        public double Lat { get; set; }
        public EventHandler MapsExec { get; set; }
        public EventHandler Justificativa { get; set; }
        public EventHandler CheckIn { get; set; }
        public List<ListTypePDV> listTypePdv;
    }
}

