using System;
using Newtonsoft.Json;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile
{
	public class PontoEletronicoRestCon
	{
		/// <summary>
		/// Get ponto eletronico Serializado.
		/// </summary>
		/// <returns>ponto eletronico serializado</returns>
		/// <param name="table">Table.</param>
		internal string GetObjectPontoEletronico(TB_USUARIO table)
		{

			var obj = new PontoEletronicoRestModel();
            if (string.IsNullOrEmpty(table.CHK_IN)) { return null; }
            obj.entrada = table.CHK_IN;
            obj.saida = table.CHK_OUT;
            obj.chegadaAlmoco = table.ALMOCO_FIM;
            obj.saidaAlmoco = table.ALMOCO_INICIO;
			return JsonConvert.SerializeObject(obj);
		}
	}
}

