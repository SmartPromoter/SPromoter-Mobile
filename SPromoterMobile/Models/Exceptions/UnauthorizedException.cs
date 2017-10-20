using System;

namespace SPromoterMobile.Models.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base ("UnauthorizedException"){ }
    }
}
