using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class GameEndDto
	{
		public List<PlayerInventoryDto> Wallets { get; set; }

		public GameEndDto(List<PlayerInventoryDto> wallets)
		{
			Wallets = wallets;
		}
	}
}
