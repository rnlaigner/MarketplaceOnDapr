﻿namespace ProductMS.Infra;

public class ProductConfig
{
	public bool Streaming { get; set; } = false;

    public bool InMemoryDb { get; set; } = false;

	public bool PostgresEmbed { get; set; } = false;

    public bool Logging { get; set; } = false;

    public string RamDiskDir { get; set; } = "";
}
