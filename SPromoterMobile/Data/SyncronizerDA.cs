using Newtonsoft.Json;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using static SPromoterMobile.Models.RESTful.FormSchemasRestModel;
using static SPromoterMobile.Models.RESTful.ProdutosRestModel;
using SQLite;
using SPromoterMobile.Models;
using System.Globalization;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers = true)]
    public class SyncronizerDA
    {
        internal readonly SQLiteAsyncConnection database;

        /// <summary>
        /// Initializa uma nova instancia de <see cref="T:SPromoterMobile.Data.SyncronizerDA"/> .
        /// </summary>
        /// <param name="database">Database.</param>
        public SyncronizerDA(SQLiteAsyncConnection database)
        {
            this.database = database;
        }


        /// <summary>
        /// Get formularios para enviar.
        /// </summary>
        /// <returns>lista de formularios.</returns>
        internal List<TB_TAREFAS> GetFormToSend(string idUser, string idPDV)
        {

            object[] parametros = { idUser, (int)StatusAPI.CONCLUIDO, (int)StatusAPI.INICIADO, (int)StatusAPI.INICIADO, (int)StatusAPI.CONCLUIDO, idPDV };
            return database.QueryAsync<TB_TAREFAS>("SELECT FORM.* FROM TB_TAREFAS FORM " +
                                                   " INNER JOIN TB_VISITA VISITA ON FORM.VISITA_ID == VISITA.ID " +
                                                   " WHERE  VISITA.ID_USER_RELACIONADO == ? AND ( FORM.STATUS = ? OR FORM.STATUS = ? ) AND " +
                                                           " ( FORM.STATUSCACHE == ? OR FORM.STATUSCACHE == ? ) " +
                                                   "AND VISITA.ID = ?", parametros).Result;
        }

        internal List<TB_VISITA> GetIDLojasToSend(string idUser)
        {

            object[] parametros = { idUser, (int)StatusAPI.CONCLUIDO, (int)StatusAPI.INICIADO, (int)StatusAPI.INICIADO, (int)StatusAPI.CONCLUIDO };
            return database.QueryAsync<TB_VISITA>("SELECT DISTINCT VISITA.ID FROM TB_TAREFAS FORM " +
                                   " INNER JOIN TB_VISITA VISITA ON FORM.VISITA_ID == VISITA.ID " +
                                   " WHERE  VISITA.ID_USER_RELACIONADO == ? AND ( FORM.STATUS = ? OR FORM.STATUS = ? ) AND " +
                                   " ( FORM.STATUSCACHE == ? OR FORM.STATUSCACHE == ? ) ", parametros).Result;
        }


        /// <summary>
        /// Set LogOff do usuario logado
        /// </summary>
        public void SetLogOff()
        {
            var genericDA = new GenericActDA(database);
            var users = SelectInfoDeUsuarios();
            if (users != null)
            {
                foreach (var item in users)
                {
                    genericDA.RemoveUser(item.ID);
                }
            }
        }

        public void RemoveUser(string userID)
        {
            var genericDA = new GenericActDA(database);
            genericDA.RemoveUser(userID);
        }

        public List<TB_USUARIO> SelectInfoDeUsuarios()
        {
            return database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE ATIVO = ? ", true).Result;
        }


        //todo: validar isto aqui
        /// <summary>
        /// Atualiza um determinado PDV anteriormente iniciado , justificado ou concluido como concluido..(wat?)
        /// </summary>
        /// <param name="id">Identifier.</param>
        internal void AtualizaVisita(string id)
        {
            object[] parametros = { (int)StatusAPI.INICIADO, (int)StatusAPI.JUSTIFICADO, (int)StatusAPI.CONCLUIDO, id };
            var queryResult = database.QueryAsync<TB_VISITA>("SELECT ID FROM TB_VISITA WHERE ( STATUS = ? OR STATUS = ? OR STATUS = ? ) AND ID = ?", parametros).Result.FirstOrDefault();
            if (queryResult != null)
            {
                queryResult.STATUS = (int)StatusAPI.CONCLUIDO;
                database.UpdateAsync(queryResult);
            }
        }

        /// <summary>
        /// Atualiza cache de notificacao de pdvs.
        /// </summary>
        /// <param name="id">Identifier PDV</param>
        internal void AtualizaVisitaNotificacao(string id)
        {
            var queryResult = database.GetAsync<TB_VISITA>(id).Result;
            if (queryResult != null)
            {
                queryResult.NOTIFICADO = true;
                database.UpdateAsync(queryResult);
            }
        }

        /// <summary>
        /// Atualiza o status do formulario de visao da loja
        /// </summary>
        /// <param name="id">Identifier pdv.</param>
        /// <param name="statusPesquisa">Status a ser pesquisado na query.</param>
        /// <param name="statusUpdate">Status a ser atualizado na query.</param>
        internal void UpdateFormsStatus(string id, StatusAPI statusPesquisa, StatusAPI statusUpdate)
        {
            object[] parametros = { id, (int)statusPesquisa };
            var querySelect = database.QueryAsync<TB_TAREFAS>("SELECT VISITA_ID , PRODUTO_ID FROM TB_TAREFAS" +
                                                                  " WHERE  (ID_SERVER_FORM = ? AND STATUS = ? )", parametros).Result.FirstOrDefault();
            if (querySelect != null)
            {
                object[] paramUpdate = { (int)statusUpdate, querySelect.PRODUTO_ID, querySelect.VISITA_ID };
                database.QueryAsync<TB_TAREFAS>("UPDATE TB_TAREFAS SET STATUS = ? WHERE PRODUTO_ID = ? AND VISITA_ID = ?", paramUpdate).Wait();
            }
        }

        internal bool HasVisitasInDate(DateTime date, string iD)
        {
            var currentDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            object[] param = { currentDate, iD };
            var querySelect = database.QueryAsync<TB_VISITA>("SELECT ID FROM TB_VISITA" +
                                                              " WHERE  DATA_PROGAMADA = ? AND ID = ?", param).Result;
            return querySelect.Count >= 0;
        }

        internal void UpdateFormToSended(List<TB_TAREFAS> tarefas)
        {
            foreach (var item in tarefas)
            {
                if (item.STATUSCACHE != (int)StatusAPI.ENVIADO &&
                    item.STATUS == (int)StatusAPI.CONCLUIDO)
                {
                    item.STATUS = (int)StatusAPI.ENVIADO;
                }
                item.STATUSCACHE = (int)StatusAPI.ENVIADO;
                database.UpdateAsync(item).Wait();
            }
        }


        /// <summary>
        /// Seleciona a tabela de ponto eletronico
        /// </summary>
        /// <returns>Row do ponto eletronico do usuario logado.</returns>
        /// <param name="idUser">Identifier user.</param>
        public TB_USUARIO SelectCheckinCheckOut(string idUser)
        {
            object[] parametros = { idUser };
            return database.QueryAsync<TB_USUARIO>(
                "SELECT * FROM TB_USUARIO WHERE ID = ? ",
                parametros).Result.FirstOrDefault();
        }

        /// <summary>
        /// Dropa todas as principais tabelas ( menos a de info de usuario ) e cria todas novamente.
        /// </summary>
        internal void ResetPontoEletronico_Cache(string idUser)
        {
            var users = database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE ID = ? ", idUser).Result;
            if (users != null)
            {
                foreach (var item in users)
                {
                    var user = SelectCheckinCheckOut(item.ID);
                    user.CHK_IN = null;
                    user.CHK_OUT = null;
                }
                database.UpdateAllAsync(users).Wait();
            }
        }

        /// <summary>
        /// Insere uma lista de produtos no banco de dados.
        /// </summary>
        /// <param name="produtos">lista de produtos.</param>
        public void InsertProduto(List<ProdutoModel> produtos)
        {
            var values = new List<TB_PRODUTO>();
            foreach (var produto in produtos)
            {
                var queryResult = database.QueryAsync<TB_VISITA>("SELECT ID FROM TB_PRODUTO WHERE ID = ? ", produto.id).Result;
                if (queryResult.Count == 0)
                {
                    var item = new TB_PRODUTO()
                    {
                        ID = produto.id,
                        NOME = produto.nome,
                        CATEGORIA = produto.categoria.nome
                    };
                    values.Add(item);
                }
            }
            database.InsertAllAsync(values).Wait();
        }


        internal void InsertVisitaProdutosForms(List<Formularios> forms, string visitaId, string geoPT)
        {
            var valuesToInsert = new List<TB_TAREFAS>();
            foreach (var form in forms)
            {
                var item = new TB_TAREFAS()
                {
                    PRODUTO_ID = form.idProduto,
                    ID_SERVER_FORM = form.idFormulario,
                    ID_FORM_SCHEMA = form.idFormSchema,
                    FORMULARIO = GetLastForm(form.idFormSchema, form.idProduto, geoPT),
                    VISITA_ID = visitaId,
                    STATUS = (int)StatusAPI.NAO_INICIADO
                };
                object[] param = { form.idProduto, visitaId };
                var queryResult = database.QueryAsync<TB_TAREFAS>("SELECT PRODUTO_ID FROM TB_TAREFAS" +
                                                                                " WHERE PRODUTO_ID = ? AND VISITA_ID = ? ", param).Result;

                if (queryResult.Count == 0)
                {
                    //"00000000-0000-0000-0000-000000000000"
                    valuesToInsert.Add(item);
                }
            }
            database.InsertAllAsync(valuesToInsert).Wait();
        }


        internal string GetLastForm(string formSchema, string produtoid, string geopt)
        {
            object[] param = { produtoid, geopt };
            var queryResult = database.QueryAsync<TB_TAREFAS>("SELECT TAREFAS.FORMULARIO FROM TB_TAREFAS TAREFAS" +
                    " INNER JOIN TB_VISITA VISITA ON VISITA.ID = TAREFAS.VISITA_ID" +
                    " WHERE TAREFAS.PRODUTO_ID = ? AND VISITA.ENDERECO = ?", param).Result;
            if (queryResult != null && queryResult.Any())
            {
                return queryResult.LastOrDefault().FORMULARIO;
            }
            return database.GetAsync<TB_TYPE_FORMS>(formSchema).Result.FORM;
        }

        /// <summary>
        /// Insere um pdv no roteiro.
        /// </summary>
        /// <returns><c>true</c>, se o pdv foi inserido, <c>false</c> se o pdv foi atualizado.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="date">Date.</param>
        /// <param name="geoPT">Endereco completo e formatado do pdv</param>
        /// <param name="lat">Lat.</param>
        /// <param name="longi">Longi.</param>
        /// <param name="tbFormServerID">lista de schemas.</param>
        public bool InsertVisitas(string id, DateTime date, string geoPT,
            double lat, double longi, List<Formularios> tbFormServerID, string userID)
        {
            bool result = false;
            var dtTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var queryResult = database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE ID = ? ", id).Result;
            if (queryResult.Count != 0)
            {
                var values = queryResult.FirstOrDefault();
                values.ID = id;
                values.DATA_PROGAMADA = dtTime.ToString("yyyy-MM-dd HH:mm");
                values.LAT = 0;
                values.LONG = 0;
                values.LAT_PDV = lat;
                values.LONG_PDV = longi;
                values.ENDERECO = geoPT;
                values.ID_USER_RELACIONADO = userID;
                database.UpdateAsync(values).Wait();
            }
            else
            {
                var isNewPdv = database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE ENDERECO LIKE ? ", geoPT).Result;
                if (isNewPdv.Count <= 0)
                {
                    result = true;
                }
                var values = new TB_VISITA()
                {
                    ID = id,
                    DATA_PROGAMADA = dtTime.ToString("yyyy-MM-dd HH:mm"),
                    LAT = 0,
                    LONG = 0,
                    LAT_PDV = lat,
                    LONG_PDV = longi,
                    ENDERECO = geoPT,
                    ID_USER_RELACIONADO = userID,
                    STATUS = (int)StatusAPI.NAO_INICIADO
                };
                database.InsertAsync(values).Wait();
            }
            InsertVisitaProdutosForms(tbFormServerID, id, geoPT);
            return result;
        }

        internal TB_VISITA GetPdvInfo(TB_VISITA pdv)
        {
            return database.GetAsync<TB_VISITA>(pdv.ID).Result;
        }

        /// <summary>
        /// Get visitas justificadas.
        /// </summary>
        /// <returns>Lista de visitas justificadas.</returns>
        public List<TB_VISITA> GetVisitasJustificadas(string idUser)
        {
            object[] param = { (int)StatusAPI.JUSTIFICADO, idUser };
            return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE STATUS = ? AND ID_USER_RELACIONADO = ?", param).Result;
        }

        /// <summary>
        /// Get todas as informacoes de um pdv (visita).
        /// </summary>
        /// <returns>Row da visita selecionada na tabela</returns>
        /// <param name="idPdv">Identifier pdv.</param>
        internal TB_VISITA GetVisita(string idPdv)
        {
            return database.GetAsync<TB_VISITA>(idPdv).Result;
        }

        /// <summary>
        /// GET id do Schema
        /// </summary>
        /// <returns>id do Schema.</returns>
        /// <param name="visitaID">Visita identifier.</param>
        /// <param name="produtoID">Produto identifier.</param>
        internal string GetSchemaID(string visitaID, string produtoID)
        {
            object[] param = { visitaID, produtoID };
            return database.QueryAsync<TB_TAREFAS>("SELECT ID_SERVER_FORM FROM TB_TAREFA " +
                                                   "WHERE VISITA_ID == ? AND PRODUTO_ID == ? ", param).Result.FirstOrDefault().ID_SERVER_FORM;
        }

        /// <summary>
        /// Select lista de PDVs pelo status.
        /// </summary>
        /// <returns>Lista de pdvs</returns>
        /// <param name="status">Status.</param>
        internal List<TB_VISITA> SelectPDVs(StatusAPI status, string idUserRelacionado)
        {
            object[] param = { (int)status, false, idUserRelacionado };
            return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE STATUS = ? " +
                                                  " AND NOTIFICADO = ? AND ID_USER_RELACIONADO = ?", param).Result;

        }

        /// <summary>
        /// Deleta visitas no banco de dados local que nao esta na lista do servidor
        /// </summary>
        /// <param name="visitasIdsInseridos">Visitas identifiers inseridos.</param>
        internal void ConluirVisitasNotInService(List<string> visitasIdsInseridos, string userLogged)
        {
            if (visitasIdsInseridos.Count > 0)
            {
                var hasChanged = false;
                var param = new object[] { userLogged, (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO };
                var list = database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE ID_USER_RELACIONADO = ? AND ( STATUS = ? OR STATUS = ?)", param).Result;
                foreach (TB_VISITA visita in list)
                {
                    if (!visitasIdsInseridos.Contains(visita.ID))
                    {
                        if (visita.STATUS == (int)StatusAPI.NAO_INICIADO &&
                            DateTime.Now.DayOfYear != DateTime.ParseExact(visita.INICIO, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).DayOfYear)
                        {
                            visita.LAT = 0;
                            visita.LONG = 0;
                            visita.INICIO = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                            visita.FIM = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                            visita.STATUS = (int)StatusAPI.JUSTIFICADO;
                            visita.JUSTIFICATIVA = "PDV nao iniciado pelo promotor(a)";
                        }
                        else
                        {
                            visita.STATUS = (int)StatusAPI.CONCLUIDO;
                        }
                        hasChanged = true;
                    }
                }
                if (hasChanged)
                {
                    database.UpdateAllAsync(list).Wait();
                }
            }
        }

        /// <summary>
        /// GET Pdvs fora da data estipulada no parametro
        /// </summary>
        /// <returns>PDVs diferentes do parameteo setado.</returns>
        /// <param name="dt">data de referencia.</param>
        internal List<TB_VISITA> GetPdvsAntigos(DateTime dt)
        {
            object[] param = { dt };
            return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE DT_ENVIO != ? ", param).Result;
        }

        /// <summary>
        /// Update visita enviadas.
        /// </summary>
        /// <param name="item">Item.</param>
        internal void UpdateVisitaEnviado(TB_VISITA item)
        {
            database.UpdateAsync(item).Wait();
        }

        /// <summary>
        /// Atualiza as informacoes do usuario logado
        /// </summary>
        /// <param name="userInfo">User info.</param>
        public void AtualizaUserInfo(TB_USUARIO userInfo)
        {
            database.UpdateAsync(userInfo).Wait();
        }

        /// <summary>
        /// Insere estrutura de um schemas na base de dados.
        /// </summary>
        /// <param name="schemas">Schemas.</param>
        public void InsertSchema(List<FormSchema> schemas)
        {
            var values = new List<TB_TYPE_FORMS>();
            foreach (var form in schemas)
            {
                var queryResult = database.QueryAsync<TB_TYPE_FORMS>("SELECT ID FROM TB_TYPE_FORMS WHERE ID = ? ", form.ID).Result;
                if (queryResult.Count == 0)
                {
                    var item = new TB_TYPE_FORMS()
                    {
                        ID = form.ID,
                        FORM = JsonConvert.SerializeObject(form)
                    };
                    values.Add(item);
                }
            }
            database.InsertAllAsync(values).Wait();
        }

        internal void AtualizaVisitaNotificacaoConcluido(string iD)
        {
            var queryResult = database.GetAsync<TB_VISITA>(iD).Result;
            queryResult.STATUS = (int)StatusAPI.ENVIADO;
            queryResult.NOTIFICADO = true;
            database.UpdateAsync(queryResult).Wait();
        }
    }
}

