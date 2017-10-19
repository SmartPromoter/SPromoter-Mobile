// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace SmartPromoter.Iphone
{
    [Register ("FeedPDVCell")]
    partial class FeedPDVCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnCheckin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnJustificativa { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnLoja { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView checkin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView contentView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel endereco { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView justificariocn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnCheckin != null) {
                btnCheckin.Dispose ();
                btnCheckin = null;
            }

            if (btnJustificativa != null) {
                btnJustificativa.Dispose ();
                btnJustificativa = null;
            }

            if (btnLoja != null) {
                btnLoja.Dispose ();
                btnLoja = null;
            }

            if (checkin != null) {
                checkin.Dispose ();
                checkin = null;
            }

            if (contentView != null) {
                contentView.Dispose ();
                contentView = null;
            }

            if (endereco != null) {
                endereco.Dispose ();
                endereco = null;
            }

            if (justificariocn != null) {
                justificariocn.Dispose ();
                justificariocn = null;
            }
        }
    }
}