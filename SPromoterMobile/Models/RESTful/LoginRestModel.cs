using Newtonsoft.Json;

namespace SPromoterMobile.Models.RESTful
{
    [Preserve(AllMembers =true)] 
	public class LoginRestModel
    {
        public LoginRestModel()
        {}

        [JsonProperty("result")]
        public Result Result { get; set; }
    }
    [Preserve(AllMembers =true)] 
	public class Result
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("nome")]
        public string Nome { get; set; }
        [JsonProperty("cargo")]
        public string Cargo { get; set; }
    }
}
