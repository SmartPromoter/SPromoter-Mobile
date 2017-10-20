using System;
using System.Collections.Generic;
using System.Linq;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Tables;
using SQLite;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class MenuTarefasDA : GenericActDA
	{

		/// <summary>
		/// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.MenuTarefasDA"/> .
		/// </summary>
		/// <param name="database">Database.</param>
		public MenuTarefasDA(SQLiteAsyncConnection database) : base(database) { }

		/// <summary>
		/// Set check out do PDV no banco de dados.
		/// </summary>
		/// <param name="pdvs">Identifier PDV.</param>
		public void SetCheckOut(List<ListTypePDV> pdvs)
		{
			foreach (var item in pdvs)
			{
				var data = database.GetAsync<TB_VISITA>(item.IdVisita).Result;
				data.STATUS = (int)StatusAPI.CONCLUIDO;
                data.FIM = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
				data.NOTIFICADO = false;
				database.UpdateAsync(data).Wait();
				object[] param = { StatusAPI.CONCLUIDO, item.IdVisita };
				database.QueryAsync<TB_TAREFAS>("UPDATE TB_TAREFAS SET STATUSCACHE = ?  WHERE VISITA_ID = ? ", param).Wait();
			}
		}

		/// <summary>
		/// Get lista de categorias de produtos.
		/// </summary>
		/// <returns>Lista de categorias</returns>
		/// <param name="idPdv">Identifier PDV.</param>
		public List<string> ListCategorias(string idPdv)
		{
			var listData = new List<string>();
			object[] param = { idPdv, (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO };
			var list = database.QueryAsync<TB_PRODUTO>("SELECT DISTINCT PROD.CATEGORIA FROM TB_PRODUTO  PROD " +
					" JOIN TB_TAREFA PRODVISITA ON PROD.ID = PRODVISITA.PRODUTO_ID  AND ( PRODVISITA.VISITA_ID = ? )" +
					" AND ( PRODVISITA.STATUS = ?  OR  PRODVISITA.STATUS = ? ) ORDER BY PROD.CATEGORIA COLLATE NOCASE", param).Result;

			if (list != null)
			{
				foreach (var item in list)
				{
					listData.Add(item.CATEGORIA);
				}
			}
			return listData;
		}


		/// <summary>
		/// Get lista de categorias de produtos.
		/// </summary>
		/// <returns>Lista de categorias</returns>
		/// <param name="idPdv">Identifier PDV.</param>
		public List<string> ListCategoriaByStatus(string idPdv)
		{
			var listData = new List<string>();
			object[] param = { idPdv };
			var list = database.QueryAsync<TB_PRODUTO>("SELECT DISTINCT PROD.CATEGORIA FROM TB_PRODUTO  PROD " +
					" JOIN TB_TAREFA PRODVISITA ON PROD.ID = PRODVISITA.PRODUTO_ID  AND ( PRODVISITA.VISITA_ID = ? )" +
					"  ORDER BY PROD.CATEGORIA COLLATE NOCASE", param).Result;

			if (list != null)
			{
				foreach (var item in list)
				{
					listData.Add(item.CATEGORIA);
				}
			}
			return listData;
		}

		/// <summary>
		/// Get lista de produtos.
		/// </summary>
		/// <returns>Lista de produtos.</returns>
		/// <param name="idPdv">Identifier PDV.</param>
        public List<TB_PRODUTO> ListProdutos(string idPdv)
		{
			object[] param = { idPdv, (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO };
			var list = database.QueryAsync<TB_PRODUTO>("SELECT PROD.* FROM TB_PRODUTO PROD " +
				" JOIN TB_TAREFAS PRODVISITA ON PROD.ID = PRODVISITA.PRODUTO_ID " +
				" AND ( PRODVISITA.VISITA_ID = ? ) AND ( PRODVISITA.STATUS = ? OR PRODVISITA.STATUS = ? )" +
				" ORDER BY PROD.CATEGORIA COLLATE NOCASE", param).Result;
			return list;
		}

		/// <summary>
		/// Get informacoes do produto.
		/// </summary>
		/// <returns>Row da tabela de produto</returns>
		/// <param name="nomeProduto">Nome do produto.</param>
		public List<TB_PRODUTO> GetProdutosList(string nomeProduto)
		{
			return database.QueryAsync<TB_PRODUTO>("SELECT * FROM TB_PRODUTO WHERE NOME = ? ", nomeProduto)
						   .Result;
		}

		/// <summary>
		/// Get informacoes do produto.
		/// </summary>
		/// <returns>Row da tabela de produto</returns>
		/// <param name="nomeProduto">Nome do produto.</param>
		public TB_PRODUTO GetProduto(string nomeProduto)
		{
			return database.QueryAsync<TB_PRODUTO>("SELECT * FROM TB_PRODUTO WHERE NOME = ? ", nomeProduto)
						   .Result.FirstOrDefault();
		}

		/// <summary>
		/// Get informacoes do PDV.
		/// </summary>
		/// <returns>Informacoes do PDV.</returns>
		/// <param name="idPdv">Identifier visita.</param>
		public TB_VISITA GetPDVInfo(string idPdv)
		{
			return database.QueryAsync<TB_VISITA>("SELECT * FROM TB_VISITA WHERE ID = ? ", idPdv).Result.FirstOrDefault();
		}

		/// <summary>
		/// Get informacoes do PDV
		/// </summary>
		/// <returns>Informacoes do PDV.</returns>
		/// <param name="idPdv">Identifier visita.</param>
		public TB_VISITA GetLojaInfo(string idPdv)
		{
			var infoFormVar = GetPDVInfo(idPdv);

			if (infoFormVar != null && infoFormVar.STATUS == (int)StatusAPI.CONCLUIDO ||
				 infoFormVar.STATUS == (int)StatusAPI.ENVIADO ||
				 infoFormVar.STATUS == (int)StatusAPI.JUSTIFICADO)
			{
				return null;
			}
			return infoFormVar;
		}

		public bool HasTarefaLoja(string idVisita)
		{
			object[] param = { idVisita, "00000000-0000-0000-0000-000000000000"
			, (int)StatusAPI.NAO_INICIADO, (int)StatusAPI.INICIADO };
			var query = database.QueryAsync<TB_TAREFAS>("SELECT VISITA_ID FROM TB_TAREFAS WHERE VISITA_ID = ? AND PRODUTO_ID = ? AND " +
																	  " ( STATUS = ? OR STATUS = ? )", param).Result;
			if (query != null && query.Count > 0)
			{
				return true;
			}
			return false;
		}


		public bool IsCorrectID(string idVisita, string idProduto)
		{
			object[] param = { idVisita, idProduto };
            var query = database.QueryAsync<TB_TAREFAS>("SELECT * FROM TB_TAREFAS WHERE VISITA_ID = ? AND PRODUTO_ID = ?", param).Result;
			if (query != null && query.Count > 0)
			{
				return true;
			}
			return false;
		}

		public string GetIDByVisita(string idVisita)
		{
			var query = database.QueryAsync<TB_VISITA>("SELECT ID_USER_RELACIONADO FROM TB_VISITA WHERE ID = ?", idVisita).Result.FirstOrDefault();
			if (query != null)
			{
				return query.ID_USER_RELACIONADO;
			}
			return null;
		}

        public int GetCountLojasnoTotal(string idVisita)
        {
            var query = database.QueryAsync<TB_TAREFAS>("SELECT * FROM TB_TAREFAS WHERE VISITA_ID = ?", idVisita).Result;
            if (query != null)
            {
                return query.Count;
            }
            return 0;
        }

        public int GetCountTarefasConcluidasEIniciadas(string idVisita)
        {
            object[] param = { idVisita, (int)StatusAPI.INICIADO, (int)StatusAPI.ENVIADO, (int)StatusAPI.CONCLUIDO };
            var query = database.QueryAsync<TB_TAREFAS>("SELECT * FROM TB_TAREFAS WHERE VISITA_ID = ? AND (STATUS = ? OR STATUS = ? OR STATUS = ?)", param).Result;
            if (query != null)
            {
                return query.Count;
            }
            return 0;
        }

	}
}

