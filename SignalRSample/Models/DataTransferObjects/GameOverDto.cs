using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class GameOverDto
	{
		public List<PlayerInventoryDto> Wallets { get; set; }

		public GameOverDto(List<PlayerInventoryDto> wallets)
		{
			Wallets = wallets;
		}
	}
}
