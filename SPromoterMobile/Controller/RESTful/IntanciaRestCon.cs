using Newtonsoft.Json;
using SPromoterMobile.Models.Exceptions;
using SPromoterMobile.Models.RESTful;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SPromoterMobile.Controller.RESTful
{
    public class IntanciaRestCon
    {

        /// <summary>
        /// Gets dados da intancia.
        /// </summary>
        /// <returns>Model da instancia</returns>
        /// <param name="alias">Alias</param>
        public InstanciaRestModel GetIntancia(string alias)
        {
            string url = Models.Credenciais.API_Credenciais.urlGetInstancia + alias;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    if (responseString.Equals("null"))
                    {
                        throw new InvalidServerException();
                    }
                    return JsonConvert.DeserializeObject<InstanciaRestModel>(responseString);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException();
                }
                return null;
            }
        }
    }
}
