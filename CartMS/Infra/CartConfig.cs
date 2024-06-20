﻿namespace CartMS.Infra
{
    /**
     * https://stackoverflow.com/questions/31453495/how-to-read-appsettings-values-from-a-json-file-in-asp-net-core
     * 
     */
    public class CartConfig
	{
        public bool ControllerChecks { get; set; }

        public bool Streaming { get; set; }

        public bool PostgresEmbed { get; set; } = false;

        public bool Unlogged { get; set; } = false;

        public string RamDiskDir { get; set; } = "";

        public bool CheckPriceUpdateOnCheckout { get; set; }

        public bool CheckIfProductExistsOnCheckout { get; set; }
    }
}