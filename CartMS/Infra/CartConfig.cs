using System;
namespace CartMS.Infra
{
    /**
     * https://stackoverflow.com/questions/31453495/how-to-read-appsettings-values-from-a-json-file-in-asp-net-core
     * 
     */
    public class CartConfig
	{
        public bool CheckPriceUpdateOnCheckout { get; set; }
        public bool CheckIfProductExistsOnCheckout { get; set; }
    }
}

