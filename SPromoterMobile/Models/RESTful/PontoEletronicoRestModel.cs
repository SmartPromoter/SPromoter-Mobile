using Newtonsoft.Json;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class PontoEletronicoRestModel
	{
		[JsonProperty("entrada")]
		public string entrada { get; set; }
		[JsonProperty("saida")]
		public string saida { get; set; }
		[JsonProperty("chegadaAlmoco")]
		public string chegadaAlmoco { get; set; }
		[JsonProperty("saidaAlmoco")]
		public string saidaAlmoco { get; set; }
	}
}

