using System;
using System.IO;
using SPromoterMobile.Data;
using SQLite;

namespace SmartPromoter.Iphone
{

    public sealed class Sqlite_IOS : ISQLite
    {
        static readonly object lockerObj = new object();

        static volatile Sqlite_IOS _instance;

        public readonly SQLiteAsyncConnection dataBase;

        public static Sqlite_IOS DB
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockerObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new Sqlite_IOS();
                            return _instance;
                        }
                    }
                }
                return _instance;
            }
        }

        Sqlite_IOS()
        {
            dataBase = GetConnection();
        }


        SQLiteAsyncConnection ISQLite.GetConnection() { return GetConnection(); }

        SQLiteAsyncConnection GetConnection()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libraryPath = Path.Combine(documentsPath, "..", "Library");
            var path = Path.Combine(libraryPath, "SPromoter.db3");
            return new SQLiteAsyncConnection(path);
        }
    }
}

