//
//  MenuTarefasCon.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.Collections.Generic;
using System.Globalization;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Tables;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class MenuTarefasCon
    {
        readonly MenuTarefasModel model;
        public MenuTarefasCon(MenuTarefasModel model)
        {
            this.model = model;
        }

        public void SetRuptura(string idVisita, string idProduto, double latitude, double longitude, int batery)
        {

            var modelForm = new FormDinamicoModel()
            {
                Db = new FormDinamicoDA(model.db.database)
            };
            model.formDinamico = new FormDinamicoCon(idVisita, idProduto, false, modelForm);
            model.formDinamico.SetRuptura(idProduto, idVisita, latitude, longitude, batery);
        }

        public bool HasVisitaAntigaAtiva()
        {
            try
            {
                foreach (var pdv in model.idVisitas)
                {
                    var infoPDV = model.db.GetPDVInfo(model.idVisitas[0].IdVisita);
                    var provider = DateTime.ParseExact(infoPDV.INICIO, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    if (DateTimeOffset.Now.DayOfYear != provider.DayOfYear)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public void CheckOutTarefa()
        {
            model.db.SetCheckOut(model.idVisitas);
        }

        public void CheckOutTarefaLista()
        {
            var ids = new List<ListTypePDV>();
            var visitas = model.idVisitas;
            foreach (var visita in visitas)
            {
                ids.Add(visita);
            }
            model.db.SetCheckOut(ids);
        }


        public List<TB_PRODUTO> ListProdutos(string idVisita)
        {
            return model.db.ListProdutos(idVisita);
        }

        public TB_VISITA GetLojaInfo(string idVisita)
        {
            return model.db.GetLojaInfo(idVisita);
        }

        public TB_PRODUTO GetProdutos(string description)
        {
            return model.db.GetProduto(description);
        }

        public List<TB_PRODUTO> GetProdutosList(string description)
        {
            return model.db.GetProdutosList(description);
        }

        public bool IsCorrectIDTarefaLoja(string idVisita, string idProduto)
        {
            return model.db.IsCorrectID(idVisita, idProduto);
        }

        public string GetIDByVisita(string idVisita)
        {
            var pdv = model.db.GetPDVInfo(idVisita);
            return pdv.ID_USER_RELACIONADO;
        }

        public int PercentualTarefas()
        {
            DateTimeOffset data = DateTimeOffset.Now;
            int total = 0;
            int realizado = 0;
            foreach (var pdv in model.idVisitas)
            {
                total = total + model.db.GetCountLojasnoTotal(pdv.IdVisita);
                realizado = realizado + model.db.GetCountTarefasConcluidasEIniciadas(pdv.IdVisita);
            }
            if (realizado == 0 && total != 0)
            {
                return 0;
            }
            if (total == 0 && realizado == 0)
            {
                return 0;
            }
            return (int)(realizado * 100.0f / total);
        }
    }
}

