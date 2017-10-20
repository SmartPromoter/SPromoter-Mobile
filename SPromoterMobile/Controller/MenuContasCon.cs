//
//  MenuContasCon.cs
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
    public class MenuContasCon
	{
		public readonly List<ContasModel> contas;
		public List<TB_USUARIO> usersInfo;
		readonly GenericModel model;

		public MenuContasCon()
		{
			contas = new List<ContasModel>();
			model = new GenericModel();
            usersInfo = model.dbGenericActivity.GetUsersIDsLogged();
		}

		public void ExcluirUsuario(TB_USUARIO conta)
		{
			contas.RemoveAll((obj) => obj.Usuario.Equals(conta.NOME) &&
										 obj.Servidor.Equals(
											 conta.SERVIDOR.Substring(0, conta.SERVIDOR.IndexOf(".", StringComparison.CurrentCulture))));

            usersInfo = model.dbGenericActivity.GetUsersIDsLogged();
			var user =	usersInfo.Find((obj) => obj.NOME.Equals(conta.NOME) &&
			               obj.SERVIDOR.Equals(conta.SERVIDOR));
			model.dbGenericActivity.RemoveUser(user.ID);
		}
	}
}
