﻿namespace ProductMS.Infra
{
	public class ProductConfig
	{
		public bool Streaming { get; set; } = false;

		public bool PostgresEmbed { get; set; } = false;

        public bool Unlogged { get; set; } = false;

        public string RamDiskDir { get; set; } = "";
	}
}

