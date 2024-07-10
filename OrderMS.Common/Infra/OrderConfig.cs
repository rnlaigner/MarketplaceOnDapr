namespace OrderMS.Common.Infra
{
	public sealed class OrderConfig
	{
        public bool Streaming { get; set; } = false;

        public bool InMemoryDb { get; set; } = false;

        public bool PostgresEmbed { get; set; } = false;

        public bool Unlogged { get; set; } = false;

        public string RamDiskDir { get; set; } = "";

    }
}

