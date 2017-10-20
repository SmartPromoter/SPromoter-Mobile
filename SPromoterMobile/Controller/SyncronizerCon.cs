using Newtonsoft.Json;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Enums;
using SPromoterMobile.Models.Exceptions;
using SPromoterMobile.Models.RESTful;
using SPromoterMobile.Models.Tables;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using static SPromoterMobile.Models.RESTful.FormSchemasRestModel;
using SPromoterMobile.Data;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using SPromoterMobile.Models.Credenciais;

namespace SPromoterMobile.Controller
{
    [Preserve(AllMembers = true)]
    public class SyncronizerCon
    {
        static List<TokenList> token = new List<TokenList>();
        List<string> idsPdvsNovos = new List<string>();
        List<TB_USUARIO> userInfos;
        static SyncronizerModel model;
        public string exceptionMessage = "";
        public InvalidLoginException invalidException;

        public SyncronizerCon(SyncronizerModel modelConst)
        {
            model = new SyncronizerModel()
            {
                db = modelConst.db,
                dbCache = modelConst.dbCache,
                versionName = modelConst.versionName
            };
            userInfos = model.db.SelectInfoDeUsuarios();
        }

        /// <summary>
        /// Inicializa uma nova instancia de <see cref="T:SPromoterMobile.Controller.SyncronizerCon"/>.
        /// </summary>
        /// <param name="cache">Model com DbCache inicializado.</param>
        public SyncronizerCon(CacheDA cache)
        {
            model = new SyncronizerModel
            {
                dbCache = cache
            };
        }

        /// <summary>
        /// Get Novos PDVs inseridos na base local
        /// </summary>
        /// <returns>Lista de novos pdvs inseridos.</returns>
        public List<string> GetNovosPdvsNotification()
        {
            var result = idsPdvsNovos;
            idsPdvsNovos = new List<string>();
            return result;
        }

        #region Gets
        void GetProdutos(TB_USUARIO userInfo)
        {
            try
            {
                var produtos = GetRest("api/mobile/produtos", Cache.PRODUTOS, userInfo, 3);
                var request = JsonConvert.DeserializeObject<ProdutosRestModel>(produtos).result;
                model.db.InsertProduto(request);
            }
            catch (NotModifiedException x)
            {
                if (x.typeofExeption == HttpStatusCode.NotFound)
                {
                    model.dbCache.UpdateCache(Cache.PRODUTOS, null, userInfo.ID);
                }
            }
        }

        void GetVisitas(TB_USUARIO userInfo)
        {
            var visitasIDs = new List<string>();
            if (model.db.HasVisitasInDate(DateTime.Now, userInfo.ID))
            {
                model.dbCache.UpdateCache(Cache.VISITAS_ATUALIZADAS, null, userInfo.ID);
            }
            try
            {
                var visitaCon = new PDVsRestCon();
                var visitas = GetRest("api/mobile/funcionarios/" + userInfo.ID + "/visitas/" + DateTime.Now.ToString("yyyy-MM-dd"),
                                      Cache.VISITAS_ATUALIZADAS, userInfo, 3);
                GetProdutos(userInfo);
                GetSchemasForms(userInfo);
                var request = JsonConvert.DeserializeObject<PDVsRestModel>(visitas).result;
                foreach (PdvRestModel item in request)
                {
                    if (item.formularios.Count > 0)
                    {
                        visitasIDs.Add(item.id);
                        var isNewPDV = model.db.InsertVisitas(item.id,
                              DateTime.Now,
                              visitaCon.GetGEO_PT(item),
                              item.lat, item.lng,
                              item.formularios,
                              userInfo.ID);
                        if (isNewPDV)
                        {
                            idsPdvsNovos.Add(visitaCon.GetGEO_PT(item));
                        }
                    }
                }
            }
            catch (Exception x)
            {
                if (!(x is NotModifiedException || x is TaskCanceledException))
                {
                    exceptionMessage = x.Message;
                }
                else if (x is InvalidLoginException)
                {
                    exceptionMessage = x.Message;
                    invalidException = (InvalidLoginException)x;
                    throw x;
                }
            }
            model.db.ConluirVisitasNotInService(visitasIDs, userInfo.ID);
        }

        void GetSchemasForms(TB_USUARIO userInfo)
        {
            try
            {
                var visitas = GetRest("api/mobile/formularios/schemas", Cache.TYPE_FORM, userInfo, 3);
                var request = JsonConvert.DeserializeObject<RestFormSchemasModel>(visitas).result;
                model.db.InsertSchema(request);
            }
            catch (Exception x)
            {
                if (!(x is NotModifiedException || x is TaskCanceledException))
                {
                    exceptionMessage = x.Message;
                }
                else if (x is InvalidLoginException)
                {
                    exceptionMessage = x.Message;
                    invalidException = (InvalidLoginException)x;
                    throw x;
                }
                else if (x is NotModifiedException)
                {
                    if (((NotModifiedException)x).typeofExeption == HttpStatusCode.NotFound)
                    {
                        model.dbCache.UpdateCache(Cache.TYPE_FORM, null, userInfo.ID);
                    }
                }
            }
        }
        #endregion Gets

        #region Sends
        internal void SendVisitasJustificadas(TB_USUARIO userInfo)
        {
            string id = userInfo.ID;
            var justificados = model.db.GetVisitasJustificadas(id);
            if (justificados != null)
            {
                foreach (var item in justificados)
                {
                    var modelVisita = new PdvRestModel
                    {
                        id = item.ID,
                        justificativa = item.JUSTIFICATIVA,
                        inicioVisita = item.INICIO,
                        fimVisita = item.FIM,
                        bateriaInicial = "0",
                        bateriaFinal = "0",
                        statusVisita = (int)StatusVisitaServer.CANCELADO,
                        lat = item.LAT,
                        lng = item.LONG
                    };
                    var obj = JsonConvert.SerializeObject(modelVisita);
                    PutRest("api/mobile/visitas/" + item.ID, obj, userInfo, 3);
                    item.STATUS = (int)StatusAPI.ENVIADO;
                }
                model.db.database.UpdateAllAsync(justificados).Wait();
            }
        }

        public void SendPontoEletronico(TB_USUARIO userInfo)
        {
            try
            {
                VerifyResetPontoEletronico(DateTime.Now, userInfo.ID);
                var pontoEletronicoCon = new PontoEletronicoRestCon();
                var infoTable = model.db.SelectInfoDeUsuarios();
                string modelCheckin;
                if (infoTable.Count > 0)
                {
                    var currentUser = infoTable[infoTable.FindIndex((obj) => obj.ID.Equals(userInfo.ID))];
                    modelCheckin = pontoEletronicoCon.GetObjectPontoEletronico(currentUser);
                    if (modelCheckin != null)
                    {
                        PutRest("api/funcionarios/" + userInfo.ID + "/pontoEletronico",
                                      Cache.PONTO_ELETRONICO, modelCheckin, userInfo, 3);
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        public void SendForms(TB_USUARIO userInfo)
        {
            var pdvsWithFormToSend = model.db.GetIDLojasToSend(userInfo.ID);
            var modelForm = new FormDinamicoModel()
            {
                Db = new FormDinamicoDA(model.db.database)
            };
            foreach (var pdv in pdvsWithFormToSend)
            {
                string objSerializado = "";
                var formsToSend = model.db.GetFormToSend(userInfo.ID, pdv.ID);
                var formController = new FormsRestCon(formsToSend, modelForm);
                var currentPDV = model.db.GetPdvInfo(pdv);
                objSerializado = formController.SerializeObject(formsToSend, currentPDV);
                PutRest("api/mobile/visitas/" + pdv.ID, objSerializado, userInfo, 3);
                formsToSend = model.db.GetFormToSend(userInfo.ID, pdv.ID);
                model.db.UpdateFormToSended(formsToSend);
            }
        }

        void SendVisitaAtualizada(TB_USUARIO userInfo)
        {
            var controllerVisitasAtualizadas = new PDVsRestCon();
            var pdvsIniciados = model.db.SelectPDVs(StatusAPI.INICIADO, userInfo.ID);
            var pdvsConcluidos = model.db.SelectPDVs(StatusAPI.CONCLUIDO, userInfo.ID);
            if (pdvsIniciados != null)
            {
                foreach (var pdvIniciado in pdvsIniciados)
                {
                    var objSerialized = controllerVisitasAtualizadas.GetSerializeAndamento(pdvIniciado);
                    PutRest("api/mobile/visitas/" + pdvIniciado.ID + "/iniciar", objSerialized, userInfo, 3);
                    model.db.AtualizaVisitaNotificacao(pdvIniciado.ID);
                }
            }
            if (pdvsConcluidos != null)
            {
                foreach (var pdvIniciado in pdvsConcluidos)
                {
                    PutRest("api/mobile/visitas/" + pdvIniciado.ID + "/concluir", userInfo, 3);
                    model.db.AtualizaVisitaNotificacaoConcluido(pdvIniciado.ID);
                }
            }
        }
        #endregion Sends

        public List<TB_USUARIO> PrepareRests()
        {
            userInfos = model.db.SelectInfoDeUsuarios();
            if (userInfos != null)
            {
                if (token == null || token.Count < userInfos.Count)
                {
                    GetToken();
                }
            }
            return userInfos;
        }

        /// <summary>
        /// Executa todos os processos do sincronizador ( Gets, Posts, Puts, envios de fotos e etc )
        /// </summary>
        public void ExecRestApis()
        {
            try
            {
                userInfos = model.db.SelectInfoDeUsuarios();
                if (userInfos != null)
                {
                    if (token == null || token.Count < userInfos.Count)
                    {
                        GetToken();
                    }
                    foreach (var userInfo in userInfos)
                    {
                        //importante v1: Nao alterar as ordems, senao vai da merda.
                        SendPontoEletronico(userInfo);
                        SendVisitasJustificadas(userInfo);
                        SendForms(userInfo);
                        SendVisitaAtualizada(userInfo);
                        //importante v2: executar os gets somente apos os puts e posts.
                        GetVisitas(userInfo);
                        //importante v3: Quero pizza.
                    }
                    exceptionMessage = "";
                    invalidException = null;
                }
            }
            catch (InvalidLoginException invalidLogin)
            {
                exceptionMessage = invalidLogin.Message;
                invalidException = invalidLogin;
                throw invalidLogin;
            }
            catch (AggregateException agr)
            {
                if (!(agr.InnerException is NotModifiedException || agr.InnerException is TaskCanceledException))
                {
                    exceptionMessage = agr.InnerException.Message;
                }
                else if (agr.InnerException is InvalidLoginException)
                {
                    exceptionMessage = agr.InnerException.Message;
                    invalidException = (InvalidLoginException)agr.InnerException;
                    throw agr;
                }
            }
            catch (Exception x)
            {
                if (!(x is NotModifiedException || x is TaskCanceledException))
                {
                    exceptionMessage = x.Message;
                }
                else if (x is InvalidLoginException)
                {
                    exceptionMessage = x.Message;
                    invalidException = (InvalidLoginException)x;
                    throw x;
                }
            }
        }

        void VerifyResetPontoEletronico(DateTime dt, string userInfo)
        {
            var pontoEletronico = model.db.SelectInfoDeUsuarios();
            if (pontoEletronico != null && pontoEletronico.Count > 0)
            {
                var checkin = pontoEletronico[pontoEletronico.FindIndex((obj) => obj.ID.Equals(userInfo))].CHK_IN;
                if ((!string.IsNullOrEmpty(checkin)) &&
                    (DateTime.ParseExact(checkin, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)).DayOfYear < dt.DayOfYear)
                {
                    model.db.ResetPontoEletronico_Cache(userInfo);
                }
            }
        }

        void GetToken()
        {
            token = new List<TokenList>();

            foreach (var userInfo in userInfos)
            {
                try
                {
                    var urlToken = "http://" + userInfo.SERVIDOR + "/token";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = new TimeSpan(0, 0, 15);
                        var prms = new Dictionary<string, string>
                                            {
                                                {"grant_type", "password"},
                                                {"userName", userInfo.LOGIN.Trim() + API_Credenciais.customMailUserDomain},
                                                {"password", userInfo.SENHA},
                                                {"X-MOBILE-REQUEST", bool.TrueString}
                                            };
                        var response = httpClient.PostAsync(urlToken, new FormUrlEncodedContent(prms)).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = response.Content;
                            string responseString = responseContent.ReadAsStringAsync().Result;
                            if (responseString.Contains("Usuário ou Senha incorreta!"))
                            {
                                throw new InvalidLoginException(userInfo.ID);
                            }

                            token.Add(new TokenList(userInfo.ID, responseString.Split('"')[3]));
                        }
                        else if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                     response.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            throw new TimeoutException();
                        }
                        else
                        {
                            throw new InvalidLoginException(userInfo.ID);
                        }
                    }
                }
                catch (NullReferenceException) { throw new InvalidLoginException(); }

            }

        }

        #region genericos
        void PostRest(string url, Cache cache, string jsonObj, TB_USUARIO userInfo, int tryCount)
        {
            if (!model.dbCache.CacheIsEqual(cache, userInfo.ID))
            {
                if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
                {
                    url = "http://" + userInfo.SERVIDOR + "/" + url;
                }
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = new TimeSpan(0, 0, 15);
                    var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                    if (currentToken == null)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                    httpClient.DefaultRequestHeaders.Add("X-App-Version", model.versionName);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                    var response = httpClient.PostAsync(url, new StringContent(jsonObj, Encoding.UTF8, "application/json")).Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    if (!response.IsSuccessStatusCode && tryCount > 0)
                    {
                        if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            throw new TimeoutException();
                        }
                        PostRest(url, cache, jsonObj, userInfo, tryCount - 1);
                    }
                    model.dbCache.UpdateCacheSync(cache, userInfo.ID);
                }
            }
        }

        void PutRest(string url, Cache cache, string jsonObj, TB_USUARIO userInfo, int tryCount)
        {
            if (!model.dbCache.CacheIsEqual(cache, userInfo.ID))
            {
                if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
                {
                    url = "http://" + userInfo.SERVIDOR + "/" + url;
                }
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = new TimeSpan(0, 0, 15);
                    var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                    if (currentToken == null)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                    httpClient.DefaultRequestHeaders.Add("X-App-Version", model.versionName);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                    var response = httpClient.PutAsync(url, new StringContent(jsonObj, Encoding.UTF8, "application/json")).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                            response.StatusCode == HttpStatusCode.GatewayTimeout)
                        {
                            throw new TimeoutException();
                        }
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            GetToken();
                            throw new UnauthorizedException();
                        }
                        if (tryCount > 0)
                        {
                            PutRest(url, cache, jsonObj, userInfo, tryCount - 1);
                        }
                        else
                        {
                            throw new HttpRequestException(response.ReasonPhrase + " endpoint: " + url);
                        }
                    }
                    model.dbCache.UpdateCacheSync(cache, userInfo.ID);
                }
            }
        }

        void PutRest(string url, TB_USUARIO userInfo, int tryCount)
        {
            if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
            {
                url = "http://" + userInfo.SERVIDOR + "/" + url;
            }
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                if (currentToken == null)
                {
                    GetToken();
                    throw new UnauthorizedException();
                }
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                httpClient.DefaultRequestHeaders.Add("X-App-Version", model.versionName);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var response = httpClient.PutAsync(url, new StringContent("", Encoding.UTF8, "application/json")).Result;
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                         response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        throw new TimeoutException();
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    if (tryCount > 0)
                    {
                        PutRest(url, userInfo, tryCount - 1);
                    }
                    else
                    {
                        throw new HttpRequestException(response.ReasonPhrase + " endpoint: " + url);
                    }
                }
            }
        }

        void PutRest(string url, string jsonObj, TB_USUARIO userInfo, int tryCount)
        {
            if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
            {
                url = "http://" + userInfo.SERVIDOR + "/" + url;
            }
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                if (currentToken == null)
                {
                    GetToken();
                    throw new UnauthorizedException();
                }
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                httpClient.DefaultRequestHeaders.Add("X-App-Version", model.versionName);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var response = httpClient.PutAsync(url, new StringContent(jsonObj, Encoding.UTF8, "application/json")).Result;
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        throw new TimeoutException();
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    if (tryCount > 0)
                    {
                        PutRest(url, jsonObj, userInfo, tryCount - 1);
                    }
                    else
                    {
                        throw new HttpRequestException(response.ReasonPhrase + " endpoint: " + url);
                    }
                }
            }
        }

        string GetRest(string url, Cache cache, TB_USUARIO userInfo, int tryCount)
        {
            try
            {
                var cacheValue = model.dbCache.GetCacheSync(cache, userInfo.ID);
                if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
                {
                    url = "http://" + userInfo.SERVIDOR + "/" + url;
                }
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = new TimeSpan(0, 0, 15);
                    var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                    if (currentToken == null)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                    if (cacheValue != null)
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", cacheValue);
                    }
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                    var response = httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;
                        model.dbCache.UpdateCache(cache, response.Headers.ETag.Tag, userInfo.ID);
                        return responseString;
                    }
                    if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        throw new TimeoutException();
                    }
                    if (response.StatusCode == HttpStatusCode.NotModified ||
                        response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new NotModifiedException(response.StatusCode);
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        GetToken();
                        throw new UnauthorizedException();
                    }
                    if (tryCount > 0)
                    {
                        return GetRest(url, cache, userInfo, tryCount - 1);
                    }
                    throw new HttpRequestException(response.ReasonPhrase + " endpoint: " + url);
                }
            }
            catch (NotModifiedException exp)
            {
                throw exp;
            }
            catch (NullReferenceException ex)
            {
                if (ex.Message != null && ex.Message.Contains("NotModified"))
                {
                    throw new NotModifiedException();
                }
                throw new HttpRequestException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        string GetRest(string url, TB_USUARIO userInfo, int tryCount)
        {
            if (!url.Contains("http://" + userInfo.SERVIDOR + "/"))
            {
                url = "http://" + userInfo.SERVIDOR + "/" + url;
            }
            using (var httpClient = new HttpClient())
            {

                httpClient.Timeout = new TimeSpan(0, 0, 15);
                var currentToken = token.Find(TokenList => (TokenList.IdUsuario.Equals(userInfo.ID)));
                if (currentToken == null)
                {
                    GetToken();
                    throw new UnauthorizedException();
                }
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + currentToken.Token);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var response = httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    return responseString;
                }
                if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                   response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    throw new TimeoutException();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    GetToken();
                    throw new UnauthorizedException();
                }
                if (tryCount > 0)
                {
                    return GetRest(url, userInfo, tryCount - 1);
                }
                throw new HttpRequestException(response.ReasonPhrase + " endpoint: " + url);
            }
        }
        #endregion genericos
    }
}
