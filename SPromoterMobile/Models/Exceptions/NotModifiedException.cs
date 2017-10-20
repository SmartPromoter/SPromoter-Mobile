using System;
using System.Net;

namespace SPromoterMobile.Models.Exceptions
{
    [Preserve(AllMembers =true)] 
    public class NotModifiedException : NullReferenceException
    {
		public HttpStatusCode typeofExeption;
        public NotModifiedException() : base ("NotModified - Cache RestFull") { }

		public NotModifiedException(HttpStatusCode code) : base ("NotModified - Cache RestFull") 
		{
			typeofExeption = code;
		}
    }
}
