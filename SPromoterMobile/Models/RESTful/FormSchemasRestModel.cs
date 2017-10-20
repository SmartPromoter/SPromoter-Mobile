using Newtonsoft.Json;
using System.Collections.Generic;

namespace SPromoterMobile.Models.RESTful
{
    [Preserve(AllMembers =true)] 
    public class FormSchemasRestModel
    {
        [Preserve(AllMembers =true)] 
        public class RestFormSchemasModel
        {
            [JsonProperty("result")]
            public List<FormSchema> result;
        }

        [Preserve(AllMembers =true)] 
        public class Result {
            [JsonProperty("formularios")]
            public List<FormSchema> formularios;
            [JsonProperty("lat")]
            public string lat;
            [JsonProperty("lng")]
            public string lng;
            [JsonProperty("inicioVisita")]
            public string inicioVisita;
            [JsonProperty("fimVisita")]
            public string fimVisita;
            [JsonProperty("bateriaInicial")]
            public string bateriaInicial;
            [JsonProperty("bateriaFinal")]
            public string bateriaFinal;
            [JsonProperty("statusVisita")]
            public int statusVisita;
        }
        [Preserve(AllMembers =true)] 
        public class FormSchema   
		{
			[JsonProperty("id")]
			public string ID { get; set; }
            [JsonProperty("nome")]
            public string nome { get; set; }
            [JsonProperty("tipo")]
            public int tipo { get; set; }
			[JsonProperty("descricoesDasFotos")]
			public List<string> fotos { get; set;}
            [JsonProperty("campos")]
            public List<Campos> campos { get; set; }
		}
        [Preserve(AllMembers =true)] 
		public class Campos   
        {
            [JsonProperty("descricao")]
            public string descricao { get; set; }
            [JsonProperty("tipo")]
            public int tipo { get; set; }
            [JsonProperty("minCaracteres")]
            public int minCaracteres { get; set; }
            [JsonProperty("maxCaracteres")]
            public int maxCaracteres { get; set; }
            [JsonProperty("ordem")]
            public int ordem { get; set; }
			[JsonProperty("opcoes")]
            public List<string> opcoes { get; set; }
            [JsonProperty("conteudo")]
			public string conteudo { get; set; }
        }
    }
}
