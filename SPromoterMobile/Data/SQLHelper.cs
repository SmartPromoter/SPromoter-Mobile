using SPromoterMobile.Models;
using SPromoterMobile.Models.Tables;
using SQLite;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers =true)] 
	public class SQLHelper
	{

		/// <summary>
		/// Inicializa o banco de dados e cria todas as tabelas.
		/// </summary>
		/// <param name="database">Database.</param>
		public void InitTables(SQLiteAsyncConnection database)
		{
            database.CreateTableAsync<TB_CACHE>().Wait();
			database.CreateTableAsync<TB_TAREFAS>().Wait();
			database.CreateTableAsync<TB_PRODUTO>().Wait();
			database.CreateTableAsync<TB_TYPE_FORMS>().Wait();
			database.CreateTableAsync<TB_USUARIO>().Wait();
			database.CreateTableAsync<TB_VISITA>().Wait();
		}


		/// <summary>
		/// Dropa todas as tabelas do banco
		/// </summary>
		/// <param name="database">Database.</param>
		public void DeleteTables(SQLiteAsyncConnection database)
		{
			database.QueryAsync<TB_CACHE>("DELETE FROM TB_CACHE").Wait();
			database.QueryAsync<TB_TAREFAS>("DELETE FROM TB_TAREFAS").Wait();
			database.QueryAsync<TB_PRODUTO>("DELETE FROM TB_PRODUTO").Wait();
			database.QueryAsync<TB_TYPE_FORMS>("DELETE FROM TB_TYPE_FORMS").Wait();
			database.QueryAsync<TB_USUARIO>("DELETE FROM TB_USUARIO").Wait();
			database.QueryAsync<TB_VISITA>("DELETE FROM TB_VISITA").Wait();
		}
	}
}
