using System;

namespace SPromoterMobile.Models.Exceptions
{
    [Preserve(AllMembers =true)] 
    public class InvalidLoginException : Exception
    {
        public string userID;
        public InvalidLoginException() : base("Usuário ou senha incorreta") { }

        public InvalidLoginException(string id) : base("Usuário ou senha incorreta")
        {
            userID = id;
        }
    }
}
