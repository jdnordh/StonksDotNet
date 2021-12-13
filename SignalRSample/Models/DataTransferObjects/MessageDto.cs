using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class MessageDto
	{
		[JsonInclude]
		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonInclude]
		[JsonProperty("color")]
		public string Color{ get; set; }
	}
}
