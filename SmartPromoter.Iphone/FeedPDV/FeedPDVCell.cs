using System;
using UIKit;
using CoreGraphics;

namespace SmartPromoter.Iphone
{
    public partial class FeedPDVCell : UICollectionViewCell
    {
        public FeedPDVCell(IntPtr handle) : base(handle) { }

        public void SetValuesCell(Pdv cell)
        {

            endereco.Text = cell.Endereco;
            btnJustificativa.TouchUpInside -= cell.Justificativa;
            btnJustificativa.TouchUpInside += cell.Justificativa;
            btnCheckin.TouchUpInside -= cell.CheckIn;
            btnCheckin.TouchUpInside += cell.CheckIn;
            btnLoja.TouchUpInside -= cell.MapsExec;
            btnLoja.TouchUpInside += cell.MapsExec;
            justificariocn.TintColor = UIColor.FromRGB(10, 88, 90);
            checkin.TintColor = UIColor.FromRGB(10, 88, 90);
            btnLoja.SetTitle(cell.NomePDV, UIControlState.Normal);

            ContentView.Layer.MasksToBounds = true;

            contentView.Layer.ShadowColor = UIColor.DarkGray.CGColor;
            contentView.Layer.ShadowOffset = new CGSize(0.0f, 1.5f);
            contentView.Layer.ShadowOpacity = 0.4f;
            contentView.Layer.MasksToBounds = false;

            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOffset = new CGSize(0.0f, 2.6f);
            Layer.ShadowOpacity = 0.6f;
            Layer.MasksToBounds = false;


        }

        public Pdv GetPdvCardUiInfo()
        {
            var tarefa = new Pdv
            {
                NomePDV = btnLoja.CurrentTitle,
                Endereco = endereco.Text
            };
            return tarefa;
        }
    }
}