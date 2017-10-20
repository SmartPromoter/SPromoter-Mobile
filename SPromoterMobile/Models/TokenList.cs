//
//  TokenList.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class TokenList
    {
        public string IdUsuario { get; set; }
        public string Token { get; set; }

        public TokenList(string idUsuario, string token)
        {
            IdUsuario = idUsuario;
            Token = token;
        }
    }
}

