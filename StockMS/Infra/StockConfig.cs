namespace StockMS.Infra
{
	public class StockConfig
	{
        public bool StockStreaming { get; set; } = false;
        public int DefaultInventory { get; set; } = 10000;
    }
}

