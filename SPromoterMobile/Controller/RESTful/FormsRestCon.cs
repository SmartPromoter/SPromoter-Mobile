using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile
{
    public class FormsRestCon
    {
        internal readonly List<FormDinamicoCon> formController;

        /// <summary>
        /// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.FormsRestCon"/> .
        /// </summary>
        /// <param name="tarefas">Identifier tarefas.</param>
        public FormsRestCon(List<TB_TAREFAS> tarefas, FormDinamicoModel model)
        {
            formController = new List<FormDinamicoCon>();
            foreach (var item in tarefas)
            {
                formController.Add(new FormDinamicoCon(item.VISITA_ID, item.PRODUTO_ID, true, model));
            }
        }


        internal string SerializeObject(List<TB_TAREFAS> tarefasTB, TB_VISITA visita)
        {
            var modelToSerialize = new FormSchemasRestModel.Result
            {
                formularios = new List<FormSchemasRestModel.FormSchema>()
            };
            foreach (var item in formController)
            {
                var form = new FormSchemasRestModel.FormSchema();
                form = item.Model.CamposForm;
                form.ID = item.Model.Id_form_server;
                form.tipo = item.Model.Tipo;
                modelToSerialize.formularios.Add(form);
            }
            if (tarefasTB != null && tarefasTB.Count > 0)
            {
                modelToSerialize.lat = tarefasTB[0].LAT.ToString().Replace(",", ".");
                modelToSerialize.lng = tarefasTB[0].LONG.ToString().Replace(",", ".");
                modelToSerialize.bateriaInicial = tarefasTB[0].BATERIA.ToString();
                modelToSerialize.bateriaFinal = tarefasTB[0].BATERIA.ToString();
            }
            else
            {
                modelToSerialize.lat = "0";
                modelToSerialize.lng = "0";
                modelToSerialize.bateriaInicial = "0";
                modelToSerialize.bateriaFinal = "0";
            }
            modelToSerialize.inicioVisita = visita.INICIO;
            if (!string.IsNullOrEmpty(visita.FIM))
            {
                modelToSerialize.fimVisita = visita.FIM;
            }
            else
            {
                modelToSerialize.fimVisita = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }
            modelToSerialize.statusVisita = (int)StatusVisitaServer.ANDAMENTO;
            return JsonConvert.SerializeObject(modelToSerialize);
        }
    }
}

