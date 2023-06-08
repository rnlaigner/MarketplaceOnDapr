using System;
namespace PaymentMS.Infra
{
	public class PaymentConfig
	{
		public bool PaymentProvider { get; set; } = false;
		public string PaymentProviderUrl { get; set; } = "";

		public bool PaymentStreaming { get; set; } = false;

		public int Delay { get; set; } = 0;
    }
}

