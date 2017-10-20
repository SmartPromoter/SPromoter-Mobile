//
//  MenuPdvsModel.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System.Collections.Generic;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class MenuPdvsModel : GenericModel
	{
		public MenuPdvsDA dbPdvs;
		public List<TB_USUARIO> infoUsuario;
	}
}

