using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class CharacterDto
	{
		[JsonInclude]
		[JsonProperty("description")]
		public string Description { get; }

		[JsonInclude]
		[JsonProperty("id")]
		public int Id { get; set; }

		public CharacterDto(string description, int id)
		{
			Description = description;
			Id = id;
		}
	}
}
