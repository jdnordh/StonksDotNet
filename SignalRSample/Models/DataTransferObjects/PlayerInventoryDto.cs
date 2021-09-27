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
		// TODO I renamed this and changed it from an int to a CharacterDto. Switch usages in the client and build errors on server side. Yeet.
		//[JsonProperty("characterId")]
		[JsonProperty("characterDto")]
		public CharacterDto CharacterDto { get; set; }

		[JsonInclude]
		[JsonProperty("money")]
		public int Money { get; }

		[JsonInclude]
		[JsonProperty("holdings")]
		public Dictionary<string, int> Holdings { get; }

		public PlayerInventoryDto(string playerId, int money, Dictionary<string, int> holdings, string username, CharacterDto characterDto)
		{
			PlayerId = playerId;
			Money = money;
			Holdings = holdings;
			Username = username;
			CharacterDto = characterDto;
		}
	}
}
