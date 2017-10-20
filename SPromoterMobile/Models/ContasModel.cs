//
//  ContasModel.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.IO;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class ContasModel
    {
        public Stream ImgEmpresa { get; set; }
        public string Servidor { get; set; }
        public string Usuario { get; set; }
        public EventHandler Excluir { get; set; }
    }
}
