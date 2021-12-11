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
		[JsonProperty("playerId")]
		public string PlayerId { get; set; }

		[JsonInclude]
		[JsonProperty("characterDto")]
		public CharacterDto CharacterDto { get; set; }

		[JsonInclude]
		[JsonProperty("shortPositionDto")]
		public ShortDto ShortPositionDto { get; set; }

		[JsonInclude]
		[JsonProperty("money")]
		public int Money { get; }

		[JsonInclude]
		[JsonProperty("visualMoney")]
		public int VisualMoney { get; }

		[JsonInclude]
		[JsonProperty("netWorth")]
		public int NetWorth { get; }

		[JsonInclude]
		[JsonProperty("holdings")]
		public Dictionary<string, int> Holdings { get; }

		public PlayerInventoryDto(string playerId, int money, int visualMoney, int netWorth, Dictionary<string, int> holdings, string username, CharacterDto characterDto, ShortDto shortPositionDto)
		{
			PlayerId = playerId;
			Money = money;
			VisualMoney = visualMoney;
			NetWorth = netWorth;
			Holdings = holdings;
			Username = username;
			CharacterDto = characterDto;
			ShortPositionDto = shortPositionDto;
		}
	}
}
