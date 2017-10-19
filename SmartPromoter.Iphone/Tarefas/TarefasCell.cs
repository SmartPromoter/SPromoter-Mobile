using System;
using UIKit;

namespace SmartPromoter.Iphone
{
    public partial class TarefasCell : UITableViewCell
    {
        public TarefasCell(IntPtr handle) : base(handle) { }

        public void SetValuesCell(Tarefa cell)
        {
            txtTarefa.Text = cell.DescricaoDaTarefa;
            txtCategoria.Text = cell.Categoria;
            if (cell.Ruptura == null)
            {
                btnRuptura.SetImage(null, UIControlState.Normal);
            }
            else
            {
                btnRuptura.SetImage(UIImage.FromBundle("rupturaIcon"), UIControlState.Normal);
                btnRuptura.TouchUpInside -= cell.Ruptura;
                btnRuptura.TouchUpInside += cell.Ruptura;
            }
        }

        public Tarefa GetTarefaInfo()
        {
            var tarefa = new Tarefa
            {
                Categoria = txtCategoria.Text,
                DescricaoDaTarefa = txtTarefa.Text
            };
            return tarefa;
        }
    }
}