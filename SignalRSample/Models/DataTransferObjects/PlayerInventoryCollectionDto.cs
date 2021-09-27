using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerInventoryCollectionDto
	{
		/// <summary>
		/// Player's inventory dtos keyed by their player id.
		/// </summary>
		[JsonInclude]
		[JsonProperty("inventories")]
		public Dictionary<string, PlayerInventoryDto> Inventories { get; set; }

		public PlayerInventoryCollectionDto(Dictionary<string, PlayerInventoryDto> inventories)
		{
			Inventories = inventories;
		}
	}
}
