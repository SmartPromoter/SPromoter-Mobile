using System.Linq;
using Newtonsoft.Json;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models.Tables;
using SQLite;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class FormDinamicoDA : GenericActDA
    {

        public FormDinamicoDA(SQLiteAsyncConnection database) : base(database) { }


        #region Formulario
        /// <summary>
        /// Get schema serializado correspondente ao id da visita referenciada.
        /// </summary>
        /// <returns>Schema serializado</returns>
        /// <param name="idVisitas">Identifier visitas.</param>
        /// /// <param name="idProduto">Identifier Produto.</param>
        internal string GetSchema(string idVisitas, string idProduto)
        {
            object[] param = { idVisitas, idProduto };
            var schemaItemTable = database.QueryAsync<TB_TAREFAS>("SELECT FORMULARIO FROM TB_TAREFA " +
                                                          " WHERE PRODUTO_ID = ? AND VISITA_ID = ?", param).Result.FirstOrDefault();
            if (schemaItemTable != null)
            {
                return schemaItemTable.FORMULARIO;
            }
            return null;
        }

        internal bool FormIsUpdated(string produtoId, string visitaId)
        {

            object[] param = { produtoId, visitaId };
            var query = database.QueryAsync<TB_TAREFAS>("SELECT * FROM TB_TAREFAS WHERE " +
                                                        " TB_PRODUTO_ID = ? " +
                                                        " AND TB_VISITA_ID = ?", param).Result.FirstOrDefault();
            if (query != null)
            {
                return true;
            }
            return false;
        }

        internal string GetCurrentUser(string idVisita)
        {
            var query = database.QueryAsync<TB_VISITA>("SELECT ID_USER_RELACIONADO FROM TB_VISITA WHERE ID = ?", idVisita).Result.FirstOrDefault();
            if (query != null)
            {
                return query.ID_USER_RELACIONADO;
            }
            return null;
        }



        /// <summary>
        /// Encontra a ultima loja realizada no mesmo endereco
        /// </summary>
        /// <returns>Id da loja</returns>
        /// <param name="idVisita">Identifier visita.</param>
        internal string GetLastIdVisita(string idVisita)
        {
            var lojaAtual = database.GetAsync<TB_VISITA>(idVisita).Result;
            var listLojas = database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE GEO_PT LIKE ?", lojaAtual.ENDERECO).Result;
            if (listLojas != null)
            {
                if (listLojas.Count > 1)
                {
                    return listLojas[listLojas.Count - 2].ID;
                }
            }
            return null;
        }



        /// <summary>
        /// Get valores do form de produto
        /// </summary>
        /// <returns>Classe populada do tipo FROMVAR</returns>
        /// <param name="idVisita">Identifier visita.</param>
        /// <param name="idProduto">Identifier produto.</param>
        internal TB_TAREFAS GetValuesForm(string idVisita, string idProduto)
        {
            object[] param = { idVisita, idProduto };
            var schemaItemTable = database.QueryAsync<TB_TAREFAS>("SELECT * FROM TB_TAREFAS WHERE VISITA_ID = ?" +
                                                          " AND PRODUTO_ID  = ?", param).Result.FirstOrDefault();
            return schemaItemTable;
        }

        /// <summary>
        /// Update de valores do formulario de produto
        /// </summary>
        /// <param name="form">Form.</param>
        /// <param name="idProd">Identifier prod.</param>
        /// <param name="idVisita">Identifier visita.</param>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        /// <param name="status">Status.</param>
        internal void InsertUpdateValues(FormSchemasRestModel.FormSchema form,
                                string idProd, string idVisita, double latitude, double longitude, StatusAPI status, int batery)
        {
            var param = new object[] { JsonConvert.SerializeObject(form), latitude, longitude, batery,
                (int)status , (int)StatusAPI.INICIADO, idProd, idVisita};
            database.QueryAsync<TB_TAREFAS>("UPDATE TB_TAREFAS SET FORMULARIO = ? , LAT = ? , LONG  = ? , BATERIA = ?" +
                                            ", STATUS = ?, STATUSCACHE = ? " +
                                            " WHERE PRODUTO_ID = ? AND VISITA_ID = ?", param).Wait();
        }
        #endregion Formulario
    }
}

