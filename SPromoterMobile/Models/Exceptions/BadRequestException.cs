using System;

namespace SPromoterMobile.Models.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException() : base ("Bad Request"){ }
    }
}
