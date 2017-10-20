using Newtonsoft.Json;

namespace SPromoterMobile.Models.RESTful
{
    [Preserve(AllMembers=true)] 
	public sealed class InstanciaRestModel
    {
		[JsonProperty("url")]
        public string url { get; set; }

    }
}
