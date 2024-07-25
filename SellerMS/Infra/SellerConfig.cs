﻿namespace SellerMS.Infra;

public sealed class SellerConfig
{
	public bool PostgresEmbed { get; set; } = false;

    public bool InMemoryDb { get; set; } = false;

    public bool Logging { get; set; } = false;

    public string RamDiskDir { get; set; } = "";
}

