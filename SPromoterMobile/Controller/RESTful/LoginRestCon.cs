using Newtonsoft.Json;
using SPromoterMobile.Models.Exceptions;
using SPromoterMobile.Models.RESTful;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using SPromoterMobile.Models.Credenciais;

namespace SPromoterMobile.Controller.RESTful
{
    public class LoginRestCon
    {
        public LoginRestCon(){ }

        /// <summary>
        /// Get informacoes do usuario.
        /// </summary>
        /// <returns>Model populada com dados do usuario</returns>
        /// <param name="servidor">Servidor.</param>
        /// <param name="pass">senha.</param>
        /// <param name="user">login de usuario.</param>
        public LoginRestModel GetLogin(string servidor, string pass, string user)
        {
            string urlLogin = "http://" + servidor + "/api/mobile/usuario?email=" + user.Replace(" ", "") + API_Credenciais.customMailUserDomain;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + GetToken(servidor, pass, user));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-MOBILE-REQUEST", bool.TrueString);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var response = httpClient.GetAsync(urlLogin).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<LoginRestModel>(responseString);
                }
                return null;
            }
        }

        string GetToken(string servidor, string pass, string user)
        {
            string urlToken = "http://" + servidor + "/token";
            using (var httpClient = new HttpClient())
            {
                var prms = new Dictionary<string, string>
                                    {
                                        {"grant_type", "password"},
                                        {"userName", user.Trim() + API_Credenciais.customMailUserDomain},
                                        {"password", pass},
                                        {"X-MOBILE-REQUEST", bool.TrueString}
                                    };

                var response = httpClient.PostAsync(urlToken, new FormUrlEncodedContent(prms)).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    if (responseString.Contains("Usuário ou Senha incorreta!"))
                    {
                        throw new InvalidLoginException();
                    }
                    return responseString.Split('"')[3];
                }
                throw new InvalidLoginException();
            }
        }
    }
}
