using Newtonsoft.Json;
using System.Collections.Generic;

namespace SPromoterMobile.Models.RESTful
{
    [Preserve(AllMembers =true)] 
    public class ProdutosRestModel
    {
       [JsonProperty("result")]
       public List<ProdutoModel> result;

    [Preserve(AllMembers =true)] 
        public class ProdutoModel
        {
            [JsonProperty("id")]
            public string id {get; set;}
            [JsonProperty("nome")]
            public string nome  {get; set;}
            [JsonProperty("marca")]
            public Marca marca {get; set;}
            [JsonProperty("precoSugerido")]
            public double precoSugerido {get; set;}
            [JsonProperty("categoria")]
            public Categoria categoria {get; set;}
        }

        [Preserve(AllMembers =true)] 
       public class Categoria
        {
            [JsonProperty("id")]
            public string id {get; set;}
            [JsonProperty("nome")]
            public string nome {get; set;}
        }

        [Preserve(AllMembers =true)] 
       public class Marca
       {
            [JsonProperty("id")]
            public string id {get; set;}
            [JsonProperty("nome")]
            public string nome {get; set;}
       }
    }
}
