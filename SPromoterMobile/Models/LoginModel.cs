using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Data;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile.Models
{
    [Preserve(AllMembers =true)] 
    public class LoginModel
    {
		public readonly int ID_REQUEST_READ_PHONE_STATE = 1;
        public LoginDA db;
        public LoginRestCon rest;
        public string login;
        public string password;
        public string empresa;
		public TB_USUARIO tableInfo;
		public bool isToAddNewUser;

        public LoginModel()
        {
        }
    }
}
