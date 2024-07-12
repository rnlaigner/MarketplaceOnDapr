namespace CustomerMS.Infra;

public class CustomerConfig
{
    public bool PostgresEmbed { get; set; } = false;

    public bool InMemoryDb { get; set; } = false;
    
    public bool Unlogged { get; set; } = false;

     public string RamDiskDir { get; set; } = "";
	
}

