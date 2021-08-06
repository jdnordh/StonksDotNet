using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class GameOverDto
	{
		[JsonInclude]
		[JsonProperty("wallets")]
		public List<PlayerInventoryDto> Wallets { get; set; }

		public GameOverDto(List<PlayerInventoryDto> wallets)
		{
			Wallets = wallets;
		}
	}
}
