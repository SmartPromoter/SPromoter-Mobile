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
    [Register ("FormDinamicoController")]
    partial class FormDinamicoController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem back { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScreenEdgePanGestureRecognizer gestureSaveForm { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UINavigationBar navigationBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView scrollViewFormDinamico { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBar tabBarFormDinamico { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabConcluir { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabFoto { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (back != null) {
                back.Dispose ();
                back = null;
            }

            if (backButton != null) {
                backButton.Dispose ();
                backButton = null;
            }

            if (gestureSaveForm != null) {
                gestureSaveForm.Dispose ();
                gestureSaveForm = null;
            }

            if (navigationBar != null) {
                navigationBar.Dispose ();
                navigationBar = null;
            }

            if (scrollViewFormDinamico != null) {
                scrollViewFormDinamico.Dispose ();
                scrollViewFormDinamico = null;
            }

            if (tabBarFormDinamico != null) {
                tabBarFormDinamico.Dispose ();
                tabBarFormDinamico = null;
            }

            if (tabConcluir != null) {
                tabConcluir.Dispose ();
                tabConcluir = null;
            }

            if (tabFoto != null) {
                tabFoto.Dispose ();
                tabFoto = null;
            }
        }

		public string IdPdv { get; set; }
		public string IdProduto { get; set; }
	}
}