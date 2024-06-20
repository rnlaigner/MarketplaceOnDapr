namespace PaymentMS.Infra
{
	public class PaymentConfig
	{
		public bool PaymentProvider { get; set; } = false;
		public string PaymentProviderUrl { get; set; } = "";

		public bool Streaming { get; set; } = false;

		public bool PostgresEmbed { get; set; } = false;

        public bool Unlogged { get; set; } = false;

        public string RamDiskDir { get; set; } = "";

		public int Delay { get; set; } = 0;
    }
}

