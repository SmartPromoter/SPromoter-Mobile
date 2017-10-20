using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Tables;
using SQLite;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers =true)] 
	public class LoginDA
	{
		readonly SQLiteAsyncConnection database;

		/// <summary>
		/// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.Data.LoginDA"/> .
		/// </summary>
		/// <param name="database">Database.</param>
		public LoginDA(SQLiteAsyncConnection database)
		{
			this.database = database;
		}

        public LoginDA()
        {
        }

        /// <summary>
        /// Insere informacoes de usuario na tabela.
        /// </summary>
        /// <param name="data">Table populada com os dados do usuario.</param>
        internal void InsertInfDUsuario(TB_USUARIO data)
		{
			if (data.AVATAR != null)
			{
                data.AVATAR_STATUS = (int)StatusAPI.CONCLUIDO;
			}
            database.InsertOrReplaceAsync(data).Wait();
		}

		/// <summary>
		/// Get informacoes de usuario.
		/// </summary>
		/// <returns>Row da tabela de usuarios</returns>
		public TB_USUARIO GetInfoUsuario()
		{
            var result = new SyncronizerDA(database).SelectInfoDeUsuarios();
            if (result == null || result.Count <= 0)
            {
                return null;
            }
            return result[0];
		}
	}
}
