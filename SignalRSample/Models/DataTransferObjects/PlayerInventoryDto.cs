using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerInventoryDto
	{
		[JsonInclude]
		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonInclude]
		[JsonProperty("money")]
		public int Money { get; }

		[JsonInclude]
		[JsonProperty("holdings")]
		public Dictionary<string, int> Holdings { get; }

		public PlayerInventoryDto(int money, Dictionary<string, int> holdings, string username)
		{
			Money = money;
			Holdings = holdings;
			Username = username;
		}
	}
}
