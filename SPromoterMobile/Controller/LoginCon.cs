using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Exceptions;
using System;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile.Controller
{
    [Preserve(AllMembers =true)] 
    public class LoginCon
    {
        readonly LoginModel model;

		/// <summary>
		/// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.Controller.LoginCon"/> .
		/// </summary>
		/// <param name="model">Model.</param>
        public LoginCon(LoginModel model) { this.model = model; }

        public LoginCon()
        {
        }

        /// <summary>
        /// Realiza login de usuario no servidor.
        /// </summary>
        /// <param name="empresa">Empresa.</param>
        /// <param name="usuario">Usuario.</param>
        /// <param name="senha">Senha.</param>
        public TB_USUARIO DoLogin(string empresa, string usuario, string senha)
        {
            return DoLogin(empresa, usuario, senha, 2);
        }


        TB_USUARIO DoLogin(string empresa, string usuario, string senha, int tryAgain)
        {
            tryAgain--;
        	var resultTable = new TB_USUARIO();
        	try
        	{
        		var instancia = new IntanciaRestCon();
        		var instanciaModel = instancia.GetIntancia(empresa);
        		var result = model.rest.GetLogin(instanciaModel.url, senha, usuario);
        		resultTable.ID = result.Result.Id;
        		resultTable.NOME = result.Result.Nome;
        		resultTable.CARGO = result.Result.Cargo;
        		resultTable.LOGIN = usuario;
        		resultTable.SENHA = senha;
        		resultTable.ATIVO = true;
        		resultTable.SERVIDOR = instanciaModel.url;
        		return resultTable;
        	}
        	catch (InvalidLoginException loginError)
        	{
        		throw loginError;
        	}

        	catch (Exception ex)
        	{
                if (tryAgain > 0)
                {
                    throw ex;
                }
                return DoLogin(empresa, usuario, senha, tryAgain);
            }
        }

		public void InsertNewUser(TB_USUARIO user)
		{
			model.db.InsertInfDUsuario(user);
		}
    }
}

