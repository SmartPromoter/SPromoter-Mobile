using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models.Tables;
using Newtonsoft.Json;

namespace SPromoterMobile.Controller.RESTful
{
    public class PDVsRestCon
    {

        /// <summary>
        /// Get endereco da loja formatado.
        /// </summary>
        /// <returns>Endereco</returns>
        /// <param name="model">Model.</param>
        internal string GetGEO_PT(PdvRestModel model)
        {
            string geoPt = model.rede.nome;
            geoPt += " - " + model.loja.nome;
            geoPt += "\n" + model.endereco.logradouro;
            geoPt += ", " + model.endereco.numero;
            geoPt += "\n" + model.cidade.nome;
            geoPt += " - CEP: " + model.endereco.codigoPostal;
            return geoPt;
        }

        internal string GetSerializeAndamento(TB_VISITA pdv)
        {
            var model = new PdvRestModel()
            {
                lat = pdv.LAT,
                lng = pdv.LONG
            };
            model.bateriaInicial = model.bateriaFinal = pdv.BATERIA.ToString();
            model.inicioVisita = pdv.DATA_PROGAMADA;
            return JsonConvert.SerializeObject(model);
        }

    }
}
