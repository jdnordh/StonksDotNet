using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerInventoryCollectionDto
	{
		[JsonInclude]
		[JsonProperty("inventories")]
		public Dictionary<string, PlayerInventoryDto> Inventories { get; set; }

		public PlayerInventoryCollectionDto(Dictionary<string, PlayerInventoryDto> inventories)
		{
			Inventories = inventories;
		}
	}
}
