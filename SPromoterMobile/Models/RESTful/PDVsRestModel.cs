using Newtonsoft.Json;
using System.Collections.Generic;

namespace SPromoterMobile.Models.RESTful
{
    [Preserve(AllMembers =true)] 
    public class PDVsRestModel
    {
        [JsonProperty("result")]
        public List<PdvRestModel> result { get; set; }
    }
    [Preserve(AllMembers =true)] 
    public class PdvRestModel
    {
        [JsonProperty("id")]
        public string id {get; set;}
        [JsonProperty("inicioPrevisto")]
        public string inicioPrevisto {get; set;}
        [JsonProperty("fimPrevisto")]
        public string fimPrevisto {get; set;}
        [JsonProperty("loja")]
        public Loja loja {get; set;}
        [JsonProperty("cidade")]
        public Cidade cidade {get; set;}
        [JsonProperty("rede")]
        public Rede rede {get; set;}
        [JsonProperty("endereco")]
        public Endereco endereco {get; set;}
        [JsonProperty("formularios")]
        public List<Formularios> formularios {get; set;}
        [JsonProperty("lat")]
        public double lat {get; set;}
        [JsonProperty("lng")]
        public double lng {get; set;}
        [JsonProperty("statusVisita")]
        public int statusVisita {get; set;}
		[JsonProperty("justificativa")]
		public string justificativa {get; set;}
		[JsonProperty("bateriaInicial")]
        public string bateriaInicial {get; set;}
		[JsonProperty("bateriaFinal")]
		public string bateriaFinal {get; set;}
		[JsonProperty("inicioVisita")]
		public string inicioVisita {get; set;}
		[JsonProperty("fimVisita")]
		public string fimVisita {get; set;}
    }

    [Preserve(AllMembers =true)] 
    public class Formularios
    {
		[JsonProperty("idSchema")]
		public string idFormSchema {get; set;}
        [JsonProperty("idFormulario")]
        public string idFormulario {get; set;}
        [JsonProperty("idProduto")]
        public string idProduto {get; set;}
    }

    [Preserve(AllMembers =true)] 
    public class Loja
    {
        [JsonProperty("nome")]
        public string nome {get; set;}
    }
    [Preserve(AllMembers =true)] 
    public class Rede
    {
        [JsonProperty("nome")]
        public string nome {get; set;}
    }
    [Preserve(AllMembers =true)] 
    public class Cidade
    {
        [JsonProperty("nome")]
        public string nome {get; set;}
    }
    [Preserve(AllMembers =true)] 
    public class Endereco
    {
        [JsonProperty("logradouro")]
        public string logradouro {get; set;}
        [JsonProperty("numero")]
        public string numero {get; set;}
        [JsonProperty("codigoPostal")]
        public string codigoPostal {get; set;}
    }
}
