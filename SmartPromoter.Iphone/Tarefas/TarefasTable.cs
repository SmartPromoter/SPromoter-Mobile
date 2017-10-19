using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

namespace SmartPromoter.Iphone
{
    public partial class TarefasTable : UITableViewSource
    {

        public List<Tarefa> Tarefas { get; set; }
        public UIViewController ParentController { get; set; }

        public TarefasTable(UIViewController uiViewController)
        {
            Tarefas = new List<Tarefa>();
            ParentController = uiViewController;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Tarefas.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("tableTarefas", indexPath) as TarefasCell;
            var animal = Tarefas[indexPath.Row];
            cell.SetValuesCell(animal);
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var board = UIStoryboard.FromName("Main", null);
            var ctrl = board.InstantiateViewController("FormDinamicoController") as FormDinamicoController;
            ctrl.IdPdv = Tarefas[indexPath.Row].IdPdv;
            ctrl.IdProduto = Tarefas[indexPath.Row].IdProduto;
            ParentController.ShowViewController(ctrl, null);
            tableView.DeselectRow(indexPath, true);
        }

    }
}