using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class RollPreviewDto
	{
		[JsonInclude]
		[JsonProperty("rolls")]
		public RollDto[] Rolls{ get; set; }
	}
}
