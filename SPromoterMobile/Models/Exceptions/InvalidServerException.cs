using System;

namespace SPromoterMobile.Models.Exceptions
{
    public class InvalidServerException : Exception
    {
        public InvalidServerException() : base ("Alias do servidor nao encontrado"){ }
    }
}
