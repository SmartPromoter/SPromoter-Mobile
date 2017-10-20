using System;
using System.Linq;
using System.Collections.Generic;
using SPromoterMobile.Models.Enums;
using Newtonsoft.Json;
using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class FormDinamicoCon
    {
        public FormDinamicoModel Model { get; set; }


        /// <summary>
        /// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.FormDinamicoCon"/> .
        /// </summary>
        /// <param name="model">Model.</param>
        public FormDinamicoCon(FormDinamicoModel model)
        {
            Model = model;
            if (model.Db == null || model == null)
            {
                throw new NullReferenceException("Model ou Model.DB referenciado com null");
            }
        }




        /// <summary>
        /// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.FormDinamicoCon"/> .
        /// </summary>
        /// <param name="idVisita">Identifier visita.</param>
        /// <param name="idProduto">Identifier produto.</param>
        public FormDinamicoCon(string idVisita, string idProduto, bool isSyncMethod, FormDinamicoModel model)
        {
            Model = new FormDinamicoModel()
            {
                Db = model.Db,
                CamposForm = model.CamposForm,
                IdForm = model.IdForm,
                IdProduto = model.IdProduto,
                IdVisita = model.IdVisita,
                Id_form_server = model.Id_form_server,
                Tipo = model.Tipo
            };
            if (model.Db == null || model == null)
            {
                throw new NullReferenceException("Model ou Model.DB referenciado com null");
            }

            if (!isSyncMethod)
            {
                var tarefa = Model.Db.GetValuesForm(idVisita, idProduto);
                Model.Id_form_server = tarefa.ID_SERVER_FORM;
                Model.CamposForm = JsonConvert.DeserializeObject<FormSchemasRestModel.FormSchema>(tarefa.FORMULARIO);
                Model.Tipo = tarefa.tipo;
                var containsRuptura = Model.CamposForm.campos.FirstOrDefault(obj => obj.descricao.ToUpper().Equals("RUPTURA"));
                if (containsRuptura != null)
                {
                    Model.CamposForm.campos.FirstOrDefault(obj => obj.descricao.ToUpper().Equals("RUPTURA")).conteudo = bool.FalseString;
                }
            }
            else
            {
                var tarefa = Model.Db.GetValuesForm(idVisita, idProduto);
                Model.Id_form_server = tarefa.ID_SERVER_FORM;
                Model.CamposForm = JsonConvert.DeserializeObject<FormSchemasRestModel.FormSchema>(tarefa.FORMULARIO);
                Model.Tipo = tarefa.tipo;
            }
        }


        /// <summary>
        /// Set um formulario como ruptura.
        /// </summary>
        /// <param name="idVisita">Identifier visita.</param>
        /// <param name="idProduto">Identifier produto.</param>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public void SetRuptura(string idProduto, string idVisita, double latitude, double longitude, int batery)
        {
            var campos = Model.Db.GetValuesForm(idVisita, idProduto).FORMULARIO;
            var campoSerializado = JsonConvert.DeserializeObject<FormSchemasRestModel.FormSchema>(campos);
            foreach (var item in campoSerializado.campos)
            {
                if (item.descricao.ToUpper().Equals("RUPTURA"))
                {
                    item.conteudo = bool.TrueString;
                }
                else
                {
                    item.conteudo = null;
                }
            }
            Model.Db.InsertUpdateValues(campoSerializado, idProduto, idVisita,
                                     latitude, longitude, StatusAPI.CONCLUIDO, batery);
        }


        /// <summary>
        /// Set informacoes do formulario para tabela.
        /// </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        /// <param name="status">Status.</param>
        public void SetFormToTable(double latitude, double longitude, StatusAPI status, int batery)
        {
            Model.Db.InsertUpdateValues(Model.CamposForm, Model.IdProduto, Model.IdVisita,
                                        latitude, longitude, status, batery);
        }

        /// <summary>
        /// Get tags das fotos do formulario.
        /// </summary>
        /// <returns>The tags foto.</returns>
        public List<string> GetTagsFoto()
        {
            try
            {
                var list = Model.CamposForm.fotos.ToList();
                var resultList = new List<string>();
                foreach (var fotos in list)
                {
                    resultList.Add(fotos);
                }
                if (!list.Any())
                {
                    resultList.Add("Outros");
                }
                return resultList;
            }
            catch (NullReferenceException)
            {
                var returnWithException = new List<string>
                {
                    "Outros"
                };
                return returnWithException;
            }
        }
    }
}