using System;
using System.Collections.Generic;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class ListTypePDV
    {
        public string IdVisita { get; set; }

        public ListTypePDV() { }

        public ListTypePDV(string idVisita)
        {
            this.IdVisita = idVisita;
        }

        public string ToIntentVar(List<ListTypePDV> info)
        {
            string result = null;
            foreach (var item in info)
            {
                result += item.IdVisita + "@";
            }
            return result.Remove(result.LastIndexOf("@", StringComparison.OrdinalIgnoreCase));
        }

        public List<ListTypePDV> FromIntentVar(string info)
        {
            var result = new List<ListTypePDV>();
            var list = info.Split('@');
            foreach (var item in list)
            {
                var itemTypePdv = item.Split('|');
                var itemNew = new ListTypePDV(itemTypePdv[0]);
                result.Add(itemNew);
            }
            return result;
        }
    }
}

