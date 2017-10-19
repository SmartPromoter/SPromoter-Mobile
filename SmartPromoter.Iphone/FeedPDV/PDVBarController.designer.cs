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
    [Register ("PDVBarController")]
    partial class PDVBarController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel cargoPromotor { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UICollectionView collecFeedPdv { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel nomeUsuario { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton profileAvatar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIProgressView progressMeta { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISearchBar searchBarPDV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabAlmoco { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBar tabBarMenuPdvs { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabContas { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabSair { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabSobre { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtMeta { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtPercentMeta { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (cargoPromotor != null) {
                cargoPromotor.Dispose ();
                cargoPromotor = null;
            }

            if (collecFeedPdv != null) {
                collecFeedPdv.Dispose ();
                collecFeedPdv = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (nomeUsuario != null) {
                nomeUsuario.Dispose ();
                nomeUsuario = null;
            }

            if (profileAvatar != null) {
                profileAvatar.Dispose ();
                profileAvatar = null;
            }

            if (progressMeta != null) {
                progressMeta.Dispose ();
                progressMeta = null;
            }

            if (searchBarPDV != null) {
                searchBarPDV.Dispose ();
                searchBarPDV = null;
            }

            if (tabAlmoco != null) {
                tabAlmoco.Dispose ();
                tabAlmoco = null;
            }

            if (tabBarMenuPdvs != null) {
                tabBarMenuPdvs.Dispose ();
                tabBarMenuPdvs = null;
            }

            if (tabContas != null) {
                tabContas.Dispose ();
                tabContas = null;
            }

            if (tabSair != null) {
                tabSair.Dispose ();
                tabSair = null;
            }

            if (tabSobre != null) {
                tabSobre.Dispose ();
                tabSobre = null;
            }

            if (txtMeta != null) {
                txtMeta.Dispose ();
                txtMeta = null;
            }

            if (txtPercentMeta != null) {
                txtPercentMeta.Dispose ();
                txtPercentMeta = null;
            }
        }
    }
}