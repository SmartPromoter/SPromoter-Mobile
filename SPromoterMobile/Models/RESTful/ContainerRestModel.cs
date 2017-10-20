//
//  ContainerRestModel.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using Newtonsoft.Json;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class ContainerRestModel
	{
		[JsonProperty("result")]
		public Result result { get; set; }

	}
	public class Result
	{
		[JsonProperty("sasToken")]
		public string sasToken { get; set; }
	}
}

