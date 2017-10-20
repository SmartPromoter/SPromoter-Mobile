using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Tables;
using System;
using System.Linq;
using SQLite;
using SPromoterMobile.Models;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers = true)]
    public class CacheDA
    {
        readonly SQLiteAsyncConnection database;

        /// <summary>
        /// Initializa uma nova instancia de <see cref="T:SPromoterMobile.Data.CacheDA"/> .
        /// </summary>
        /// <param name="database">Database.</param>
        public CacheDA(SQLiteAsyncConnection database)
        {
            this.database = database;
        }

        /// <summary>
        /// Update cache.
        /// </summary>
        /// <param name="idCache">Identifier cache.</param>
        internal void UpdateCache(Cache idCache, string idUser)
        {
            var index = GetINDEX(idCache, idUser);
            var values = new TB_CACHE();
            var newCache = Environment.TickCount.ToString();
            values.CACHE = newCache;
            values.ID = (int)idCache;
            values.ID_USER_LOGGED = idUser;
            if (index > -1)
            {
                values.INDEX = index;
            }
            database.InsertOrReplaceAsync(values).Wait();
        }


        /// <summary>
        /// Update cache do sincronizador.
        /// </summary>
        /// <param name="idCache">Identifier cache.</param>
        internal void UpdateCacheSync(Cache idCache, string idUser)
        {
            var index = GetINDEX(idCache, idUser);
            var values = new TB_CACHE();
            var newCache = Environment.TickCount.ToString();
            values.CACHESYNC = values.CACHE = newCache;
            values.ID = (int)idCache;
            values.ID_USER_LOGGED = idUser;
            if (index > -1)
            {
                values.INDEX = index;
            }
            database.InsertOrReplaceAsync(values).Wait();
        }

        /// <summary>
        /// Update cache com valor da eTAG do servidor.
        /// </summary>
        /// <param name="idCache">Identifier cache.</param>
        /// <param name="eTag">Etag.</param>
        public void UpdateCache(Cache idCache, string eTag, string idUser)
        {
            if (eTag != null)
            {
                var index = GetINDEX(idCache, idUser);
                var values = new TB_CACHE()
                {
                    CACHE = eTag,
                    CACHESYNC = eTag,
                    ID = (int)idCache,
                    ID_USER_LOGGED = idUser
                };
                if (index > -1)
                {
                    values.INDEX = index;
                }
                database.InsertOrReplaceAsync(values).Wait();
            }
        }

        /// <summary>
        /// Verifica se o cache local é iqual ao cache do sincronizador.
        /// </summary>
        /// <returns><c>true</c>, se for iqual, <c>false</c> se nao for iqual.</returns>
        /// <param name="idCache">Identifier cache.</param>
        internal bool CacheIsEqual(Cache idCache, string idUser)
        {
            return GetCache(idCache, idUser).Equals(GetCacheSync(idCache, idUser));
        }

        /// <summary>
        /// Verifica se o Cache local ou do sincronizador é nullo.
        /// </summary>
        /// <returns><c>true</c>, se for nullo, <c>false</c> senao for nullo.</returns>
        /// <param name="idCache">Identifier cache.</param>
        internal bool CacheIsNull(Cache idCache, string idUser)
        {
            return (GetCache(idCache, idUser) == null || GetCacheSync(idCache, idUser) == null);
        }

        /// <summary>
        /// Get cache.
        /// </summary>
        /// <returns>Cache</returns>
        /// <param name="idCache">Identifier cache.</param>
        public string GetCache(Cache idCache, string idUser)
        {
            object[] param = { (int)idCache, idUser };
            var values = database.QueryAsync<TB_CACHE>("SELECT * FROM TB_CACHE WHERE ID = ? AND ID_USER_LOGGED = ? ", param).Result.FirstOrDefault();
            if (values == null)
            {
                return null;
            }
            return values.CACHE;
        }

        /// <summary>
        /// Get cache index
        /// </summary>
        /// <returns>Cache</returns>
        /// <param name="idCache">Identifier cache.</param>
        public int GetINDEX(Cache idCache, string idUser)
        {
            object[] param = { (int)idCache, idUser };
            var values = database.QueryAsync<TB_CACHE>("SELECT * FROM TB_CACHE WHERE ID = ? AND ID_USER_LOGGED = ? ", param).Result.FirstOrDefault();
            if (values == null)
            {
                var lastIndex = database.QueryAsync<TB_CACHE>("SELECT * FROM TB_CACHE").Result.LastOrDefault();
                if (lastIndex == null)
                {
                    return -1;
                }
                return lastIndex.INDEX + 1;
            }
            return values.INDEX;
        }



        /// <summary>
        /// Get cache do sincronizador.
        /// </summary>
        /// <returns>Cache do sincronizador.</returns>
        /// <param name="idCache">Identifier cache.</param>
        internal string GetCacheSync(Cache idCache, string idUser)
        {
            object[] param = { (int)idCache, idUser };
            var values = database.QueryAsync<TB_CACHE>("SELECT * FROM TB_CACHE WHERE ID = ? AND ID_USER_LOGGED = ? ", param).Result.FirstOrDefault();
            if (values == null)
            {
                return null;
            }
            return values.CACHESYNC;
        }
    }
}
