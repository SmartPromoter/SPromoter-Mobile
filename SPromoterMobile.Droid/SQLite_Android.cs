using System;
using System.IO;
using SPromoterMobile.Data;
using SQLite;

namespace spromotermobile.droid.Data
{

    public sealed class SQLite_Android : ISQLite
    {

        static readonly object lockerObj = new object();

        static volatile SQLite_Android _instance;


        public readonly SQLiteAsyncConnection dataBase;

        public static SQLite_Android DB
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockerObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new SQLite_Android();
                            return _instance;
                        }
                    }
                }
                return _instance;
            }
        }


        SQLite_Android()
        {
            dataBase = GetConnection();
        }


        SQLiteAsyncConnection GetConnection()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SPromoter.db3");
            return new SQLiteAsyncConnection(path);
        }


        SQLiteAsyncConnection ISQLite.GetConnection() { return GetConnection(); }

    }
}