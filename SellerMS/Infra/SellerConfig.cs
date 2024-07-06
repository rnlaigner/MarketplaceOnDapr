namespace SellerMS.Infra;

public sealed class SellerConfig
{
	public bool PostgresEmbed { get; set; } = false;

    public bool Unlogged { get; set; } = false;

    public string RamDiskDir { get; set; } = "";
}

