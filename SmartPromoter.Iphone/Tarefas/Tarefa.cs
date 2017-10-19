//
//  Tarefa.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
namespace SmartPromoter.Iphone
{
    public class Tarefa
    {
        public string Categoria { get; set; }
        public string DescricaoDaTarefa { get; set; }
        public string IdProduto { get; set; }
        public string IdPdv { get; set; }
        public EventHandler Ruptura { get; set; }
    }
}
