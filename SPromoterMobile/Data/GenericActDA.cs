using SPromoterMobile.Models.Tables;
using System;
using System.Linq;
using System.Collections.Generic;
using SPromoterMobile.Models.Enums;
using SQLite;
using SPromoterMobile.Models;
using System.Globalization;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers =true)] 
	public class GenericActDA
	{
		internal SQLiteAsyncConnection database;
		internal SyncronizerDA syncDA;
		internal CacheDA cacheDA;
		internal List<TB_USUARIO> idsLogged;

		public GenericActDA(SQLiteAsyncConnection database)
		{
			this.database = database;
			syncDA = new SyncronizerDA(database);
			cacheDA = new CacheDA(database);
			idsLogged = GetUsersIDsLogged();
		}

		/// <summary>
		/// Get primeiro ID do usuario logado
		/// </summary>
		/// <returns>ID do usuario logado</returns>
		public string GetUserIDLogged()
		{

            var userInfo = syncDA.SelectInfoDeUsuarios().FirstOrDefault();
			if (userInfo != null)
			{
				return userInfo.ID;
			}
			return null;
		}

		public TB_USUARIO GetIDUser(TB_USUARIO userInfo)
		{
			return database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE NOME = ? AND SERVIDOR LIKE ('%' || ? || '%')",
					 new object[] { userInfo.NOME, userInfo.SERVIDOR }).Result.FirstOrDefault();
		}

		/// <summary>
		/// Get todos os IDs dos usuarios logados
		/// </summary>
		/// <returns>ID do usuario logado</returns>
		public List<TB_USUARIO> GetUsersIDsLogged()
		{
			return syncDA.SelectInfoDeUsuarios();
		}

		public void RemoveUser(string idUser)
		{
            string[] argument = { idUser };
            database.ExecuteAsync("DELETE FROM TB_VISITA WHERE ID_USER_RELACIONADO = ?", argument).Wait();
            database.ExecuteAsync("DELETE FROM TB_USUARIO WHERE ID = ?", argument).Wait();
			if (GetUsersIDsLogged().Count < 1)
			{
			    throw new InvalidOperationException("Nao existem mais usuarios para serem removidos");	
			}
		}

		/// <summary>
		/// Set almoco de acordo com os dados inserido na classe do parametro
		/// </summary>
		/// <param name="table">Table.</param>
		public void SetAlmoco(TB_USUARIO table)
		{
			database.UpdateAsync(table).Wait();
            cacheDA.UpdateCache(Cache.PONTO_ELETRONICO, table.ID);
		}

		/// <summary>
		/// Get informacoes do almoco do usuario logado
		/// </summary>
		/// <returns>Classe populada com os dados do ponto eletronico ( almoco incluso )</returns>
		/// <param name="user_id">User identifier.</param>
        public TB_USUARIO GetStatusAlmoco(string user_id)
		{
            return database.QueryAsync<TB_USUARIO>("SELECT * FROM TB_USUARIO WHERE ID = ? ", user_id).Result.FirstOrDefault();
		}

		/// <summary>
		/// Verifica se o horario de almoco esta aberto.
		/// </summary>
		/// <returns><c>true</c>, se estiver em horario de almoco, <c>false</c> se nao estiver em horario de almoco.</returns>
		/// <param name="user_id">User identifier.</param>
		public bool IsHrDeAlmoco(string user_id)
		{
			var userCheck = GetStatusAlmoco(user_id);
            return userCheck != null && (!string.IsNullOrEmpty(userCheck.ALMOCO_INICIO) && 
                                         string.IsNullOrEmpty(userCheck.ALMOCO_FIM));
		}

		/// <summary>
		/// Get informacoes de almoco
		/// </summary>
		/// <returns>Lista de Datetimes com horarios de almoco inicial e final</returns>
		/// <param name="user_id">User identifier.</param>
        public List<DateTime> GetAlmoco(string user_id)
		{
			var values = new List<DateTime>();
			try
			{
				var userCheck = GetStatusAlmoco(user_id);
                if (!string.IsNullOrEmpty(userCheck.ALMOCO_INICIO))
				{
                    values.Add(DateTime.ParseExact(userCheck.ALMOCO_INICIO, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
				}
                if (!string.IsNullOrEmpty(userCheck.ALMOCO_FIM))
				{
                    values.Add(DateTime.ParseExact(userCheck.ALMOCO_FIM, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
				}
			}
			catch (NullReferenceException)
			{
				//Nao Existe ainda ponto aberto para iniciar um almoco
            }
			return values;
		}



		/// <summary>
		/// Atualiza o status de um pdv ja existente
		/// </summary>
		/// <param name="idVisita">Identifier visita.</param>
		/// <param name="prodId">Prod identifier.</param>
		/// <param name="status">Status.</param>
		internal void InsertStatusPDV(string idVisita, string prodId, StatusAPI status)
		{
            database.QueryAsync<TB_TAREFAS>("UPDATE TB_TAREFAS SET STATUS = ? " +
					 " WHERE PRODUTO_ID = ? AND VISITA_ID = ? AND ( STATUS = ? OR STATUS = ? ) ",
					  new object[] { (int)status, prodId, idVisita, (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO }).Wait();
		}

        public StatusPontoEletronico ExecPontoEletronico()
        {
        	var result = StatusPontoEletronico.NAO_INICIADO;
        	var dtHora = DateTimeOffset.Now;
            var cursor = GetUsersIDsLogged();
        		if (cursor != null)
        		{
        			foreach (var usuario in cursor)
        			{
        				var AlmocoInicio = usuario.ALMOCO_INICIO;
        				var AlmocoFim = usuario.ALMOCO_FIM;
        				var CheckOut = usuario.CHK_OUT;
        				var CheckIn = usuario.CHK_IN;
                    if (!string.IsNullOrEmpty(CheckIn) && 
                        (string.IsNullOrEmpty(AlmocoInicio) || string.IsNullOrEmpty(AlmocoFim) || string.IsNullOrEmpty(CheckOut)))
        				{
                        if (string.IsNullOrEmpty(AlmocoInicio) && string.IsNullOrEmpty(CheckOut))
        					{
                            usuario.ALMOCO_INICIO = dtHora.ToString("yyyy-MM-dd HH:mm");
        						result = StatusPontoEletronico.ALMOCO_INICIADO;
        					}
                        else if (string.IsNullOrEmpty(AlmocoFim) && string.IsNullOrEmpty(CheckOut))
        					{
                            usuario.ALMOCO_FIM = dtHora.ToString("yyyy-MM-dd HH:mm");
        						result = StatusPontoEletronico.ALMOCO_FINALIZADO;
        					}
                        else if (string.IsNullOrEmpty(CheckOut))
        					{
        						result = StatusPontoEletronico.CHECKOUT;
        					}
                            cacheDA.UpdateCache(Cache.PONTO_ELETRONICO, usuario.ID);
        				}
        				else
        				{
        					result = StatusPontoEletronico.NAO_INICIADO;
        				}
                        database.UpdateAsync(usuario).Wait();
        			}
        		}
        	return result;

        }

	}
}
