namespace StockMS.Infra;

public sealed class StockConfig
{
    public bool Streaming { get; set; } = false;

    public bool InMemoryDb { get; set; } = false;

    public bool RaiseStockFailed { get; set; } = false;

    public int DefaultInventory { get; set; } = 10000;

    public bool PostgresEmbed { get; set; } = false;

    public bool Unlogged { get; set; } = false;

    public string RamDiskDir { get; set; } = "";

}

