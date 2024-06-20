namespace ShipmentMS.Infra
{
	public class ShipmentConfig
	{
		public bool Streaming { get; set; }

		public bool PostgresEmbed { get; set; } = false;

        public bool Unlogged { get; set; } = false;

        public string RamDiskDir { get; set; } = "";
	}
}

