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
    [Register ("TarefasController")]
    partial class TarefasController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISearchBar searchBarTarefas { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBar tabBarTarefas { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabItemAlmoco { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem tabItemConcluir { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView TarefasTable { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (searchBarTarefas != null) {
                searchBarTarefas.Dispose ();
                searchBarTarefas = null;
            }

            if (tabBarTarefas != null) {
                tabBarTarefas.Dispose ();
                tabBarTarefas = null;
            }

            if (tabItemAlmoco != null) {
                tabItemAlmoco.Dispose ();
                tabItemAlmoco = null;
            }

            if (tabItemConcluir != null) {
                tabItemConcluir.Dispose ();
                tabItemConcluir = null;
            }

            if (TarefasTable != null) {
                TarefasTable.Dispose ();
                TarefasTable = null;
            }
        }
    }
}