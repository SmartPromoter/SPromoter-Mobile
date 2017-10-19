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
    [Register ("LoginController")]
    partial class LoginController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ButtonLogin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtEmpresaLogin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSenhaLogin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtUsuarioLogin { get; set; }

        [Action ("ClickLogin:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ClickLogin (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (ButtonLogin != null) {
                ButtonLogin.Dispose ();
                ButtonLogin = null;
            }

            if (txtEmpresaLogin != null) {
                txtEmpresaLogin.Dispose ();
                txtEmpresaLogin = null;
            }

            if (txtSenhaLogin != null) {
                txtSenhaLogin.Dispose ();
                txtSenhaLogin = null;
            }

            if (txtUsuarioLogin != null) {
                txtUsuarioLogin.Dispose ();
                txtUsuarioLogin = null;
            }
        }
    }
}