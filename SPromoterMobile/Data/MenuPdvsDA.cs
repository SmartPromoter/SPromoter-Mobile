using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using SPromoterMobile.Models;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers = true)]
    public class MenuPdvsDA : GenericActDA
    {
        readonly CacheDA cache;

        public MenuPdvsDA(SQLiteAsyncConnection database) : base(database)
        {
            cache = new CacheDA(database);
        }

        /// <summary>
        /// Get informacoes do usuario logado
        /// </summary>
        /// <returns>The user info logged.</returns>
        public List<TB_USUARIO> GetUserInfoLogged()
        {
            object[] param = { true };
            var list = database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE ATIVO = ? ", param).Result;
            return list;

        }

        /// <summary>
        /// Get lista de visitas pendentes
        /// </summary>
        /// <returns>Lista de visitas pendentes</returns>
        /// <param name="dt">Dt.</param>
        public List<TB_VISITA> VisitasPendentes(DateTime dt)
        {
            var dtTime = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            object[] param = { dtTime.ToString("yyyy-MM-dd HH:mm"), (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO };
            return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE DATA_PROGAMADA = ?  AND ( STATUS = ? OR STATUS  = ? ) ORDER BY ENDERECO", param).Result;
        }

        /// <summary>
        /// Get total de visitas de acordo com a data referenciada
        /// </summary>
        /// <returns>Quantidades de visitas</returns>
        /// <param name="dt">Data.</param>
        public int GetCountVisitasnoTotal(DateTime dt)
        {
            var dtTime = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            object[] param = { dtTime.ToString("yyyy-MM-dd HH:mm") };
            var list = database.QueryAsync<TB_VISITA>("SELECT ENDERECO FROM TB_VISITA WHERE DATA_PROGAMADA = ? ", param).Result;
            if (list == null)
            {
                return 0;
            }
            return list.Count();
        }

        /// <summary>
        /// Get total de visitas concluidas de acordo com a data referenciada
        /// </summary>
        /// <returns>Quantidade concluida</returns>
        /// <param name="dt">Data.</param>
        public int GetCountVisitasConcluidas(DateTime dt)
        {
            var dtTime = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            object[] param = { dtTime.ToString("yyyy-MM-dd HH:mm"), (int)StatusAPI.CONCLUIDO, (int)StatusAPI.ENVIADO, (int)StatusAPI.JUSTIFICADO };
            var list = database.QueryAsync<TB_VISITA>("SELECT ENDERECO FROM TB_VISITA WHERE DATA_PROGAMADA = ?  AND ( STATUS = ? OR STATUS  = ? OR STATUS  = ? )", param).Result;
            if (list == null)
            {
                return 0;
            }
            return list.Count();
        }

        public List<string> GetAllIdsLogged()
        {
            var result = new List<string>();
            var tableUser = GetUserInfoLogged();
            if (tableUser != null)
            {
                foreach (var item in tableUser)
                {
                    result.Add(item.ID);
                }
            }
            return result;
        }

        /// <summary>
        /// Insere a foto de perfil do usuario no banco de dados
        /// </summary>
        /// <param name="urlSFotosSalvas">URL foto salva.</param>
        /// <param name="idsUsuarios">Identifier usuario.</param>
        public void InsertFotoProfile(List<string> urlSFotosSalvas, List<string> idsUsuarios)
        {
            var rowToUpdate = new List<TB_USUARIO>();
            for (int i = 0; i < idsUsuarios.Count; i++)
            {
                var data = database.GetAsync<TB_USUARIO>(idsUsuarios[i]).Result;
                data.AVATAR = urlSFotosSalvas[i];
                data.AVATAR_STATUS = (int)StatusAPI.INICIADO;
                rowToUpdate.Add(data);
            }
            database.UpdateAllAsync(rowToUpdate).Wait();
        }

        public double[] GetGeoCodeLoja(List<ListTypePDV> idLoja)
        {
            var result = new double[2];
            var tableUser = database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE ID = ? ",
                                                           idLoja[0].IdVisita).Result.FirstOrDefault();
            if (tableUser != null)
            {
                result[0] = tableUser.LAT_PDV;
                result[1] = tableUser.LONG_PDV;
            }
            return result;
        }

        /// <summary>
        /// Set visitas em progresso
        /// </summary>
        /// <param name="listId">List identifier.</param>
        /// <param name="dataHorainicio">Data horainicio.</param>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public void SetVisitaEmProgresso(List<ListTypePDV> listId, DateTime dataHorainicio,
                                         double latitude, double longitude, int baterry)
        {
            foreach (var id in listId)
            {
                TB_VISITA visita = database.GetAsync<TB_VISITA>(id.IdVisita).Result;
                visita.INICIO = dataHorainicio.ToString("yyyy-MM-dd HH:mm");
                visita.STATUS = (int)StatusAPI.INICIADO;
                visita.LAT = latitude;
                visita.LONG = longitude;
                visita.BATERIA = baterry;
                database.UpdateAsync(visita).Wait();
            }
        }

        /// <summary>
        /// Set visitas justificadas no banco de dados
        /// </summary>
        /// <param name="listId">List identifier.</param>
        /// <param name="justificativa">Justificativa.</param>
        /// <param name="dataHora">Hora que sou informado a justificativa.</param>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public void SetVisitasJustificada(List<ListTypePDV> listId, string justificativa,
                                          DateTime dataHora, double latitude, double longitude, int batery)
        {
            foreach (var id in listId)
            {
                TB_VISITA visita = database.GetAsync<TB_VISITA>(id.IdVisita).Result;
                visita.INICIO = dataHora.ToString("yyyy-MM-dd HH:mm");
                visita.FIM = dataHora.ToString("yyyy-MM-dd HH:mm");
                visita.STATUS = (int)StatusAPI.JUSTIFICADO;
                visita.JUSTIFICATIVA = justificativa;
                visita.LAT = latitude;
                visita.LONG = longitude;
                visita.BATERIA = batery;
                database.UpdateAsync(visita).Wait();
            }
        }

        /// <summary>
        /// Efetua registro ou atualizacao do ponto eletronico
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RegistroDePonto(List<TB_USUARIO> id)
        {
            foreach (var itemID in id)
            {
                var list = database.GetAsync<TB_USUARIO>(itemID.ID).Result;
                if (list != null)
                {
                    if (string.IsNullOrEmpty(list.CHK_IN))
                    {
                        list.CHK_IN = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        database.UpdateAsync(list).Wait();
                    }
                }
                cache.UpdateCache(Cache.PONTO_ELETRONICO, itemID.ID);
            }
        }

        /// <summary>
        /// Atualiza o ponto eletronico com o checkout do usuario logado
        /// </summary>
        /// <returns><c>true</c>, se o checkout foi realizado, <c>false</c> se o checkout nao foi realizado.</returns>
        /// <param name="id">Identifier.</param>
        public bool RegistroCheckOut(List<string> id)
        {
            bool result = false;
            foreach (var item in id)
            {
                var list = database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE ID = ?", item).Result;
                if (list.Any())
                {
                    if (string.IsNullOrEmpty(list[0].CHK_OUT) &&
                        !string.IsNullOrEmpty(list[0].CHK_IN))
                    {
                        list[0].CHK_OUT = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        database.UpdateAsync(list[0]).Wait();
                        cache.UpdateCache(Cache.PONTO_ELETRONICO, item);
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get visitas  inicializadas
        /// </summary>
        /// <returns>Lista de visitas atualizadas.</returns>
        public List<TB_VISITA> GetVisitaAtual()
        {
            return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE STATUS = ?", (int)StatusAPI.INICIADO).Result;
        }
    }
}
