using System;
using Common.Events;
using CustomerMS.Models;

namespace CustomerMS.Repositories
{
	public interface ICustomerRepository
	{
        CustomerModel? GetById(long customerId);
        CustomerModel Insert(CustomerModel customer);
        CustomerModel Update(CustomerModel customer);
    }
}

