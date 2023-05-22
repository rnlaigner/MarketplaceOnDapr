using System;
using Dapr.Client;
using SellerMS.Repositories;

namespace SellerMS.Services
{
	public class SellerService : ISellerService
	{

        private readonly DaprClient daprClient;
        private readonly ISellerRepository sellerRepository;

        public SellerService(DaprClient daprClient, ISellerRepository sellerRepository)
		{

		}



	}
}

