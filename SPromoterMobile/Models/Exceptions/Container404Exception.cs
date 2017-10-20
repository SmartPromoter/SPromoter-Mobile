using System;

namespace SPromoterMobile.Models.Exceptions
{
    public class Container404Exception : NullReferenceException
    {
        public Container404Exception() : base ("Arquivo no container nao encontrado") { }
    }
}
