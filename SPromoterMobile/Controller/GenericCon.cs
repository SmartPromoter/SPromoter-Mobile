//
//  GenericCon.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.Collections.Generic;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class GenericCon
	{
		readonly GenericModel model;

		public GenericCon(GenericModel model)
		{ this.model = model; }

		public bool IsHrDeAlmoco()
		{
			try
			{
                return model.dbGenericActivity.IsHrDeAlmoco(model.dbGenericActivity.GetUsersIDsLogged()[0].ID);
			}
			catch (Exception)
			{
				return false;
			}
		}

        public List<DateTime> GetAlmoco()
		{
            return model.dbGenericActivity.GetAlmoco(model.dbGenericActivity.GetUsersIDsLogged()[0].ID);
		}

		public void RemoveUser(TB_USUARIO usuario)
		{
			model.dbGenericActivity.RemoveUser(usuario.ID);
		}


		public StatusPontoEletronico SetAlmoco()
		{
			return model.dbGenericActivity.ExecPontoEletronico();
		}
}
}

