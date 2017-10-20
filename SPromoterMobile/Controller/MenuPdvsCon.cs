//
//  MenuPdvsCon.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.Collections.Generic;
using SPromoterMobile.Models.Tables;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class MenuPdvsCon
	{
		readonly MenuPdvsModel model;
		public MenuPdvsCon(MenuPdvsModel model)
		{
			this.model = model;
		}

		public double[] GetCoordinates(List<ListTypePDV> item)
		{
			return model.dbPdvs.GetGeoCodeLoja(item);
		}

		public void CheckIn(List<ListTypePDV> item, double latitude, double longitude, int battery)
		{ 
            model.dbPdvs.SetVisitaEmProgresso(item, DateTime.Now, latitude, longitude, battery);
		}

		public void Justificativa(List<ListTypePDV> item, string justificativa,double latitude, double longitude, int batery)
		{
            model.dbPdvs.SetVisitasJustificada(item, justificativa,  DateTime.Now, latitude, longitude, batery);
		}

		public List<string> PrepareIdsUserToIntent()
		{
			var listIdsUsers = new List<string>();
			foreach (var itemUser in model.infoUsuario)
			{
				listIdsUsers.Add(itemUser.ID);
			}
			return listIdsUsers;
		}

		public void RegistroDePontoEletronico()
		{
			
			model.dbPdvs.RegistroDePonto(model.infoUsuario);
		}

		public bool CheckOutVisita(int countList)
		{
			try
			{
				if (countList < 1)
				{
					var idsUsuarios = new List<string>();
					foreach (var item in model.infoUsuario)
					{
						idsUsuarios.Add(item.ID);
					}
					if (model.dbPdvs.RegistroCheckOut(idsUsuarios))
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public List<TB_VISITA> VisitasPendentes()
		{
            return model.dbPdvs.VisitasPendentes(DateTime.Now);
		}

		public int PercentualVisitas()
		{
            var data = DateTime.Now;
			var total = model.dbPdvs.GetCountVisitasnoTotal(data);
			var realizado = model.dbPdvs.GetCountVisitasConcluidas(data);
			if (realizado == 0 && total != 0)
			{
				return 0;
			}
			if (total == 0 && realizado == 0)
			{
				return 0;
			}
			return (int)(realizado * 100.0f / total);
		}

	}
}

