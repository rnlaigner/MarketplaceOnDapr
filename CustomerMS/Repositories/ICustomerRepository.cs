using System;
using Common.Events;
using CustomerMS.Models;

namespace CustomerMS.Repositories
{
	public interface ICustomerRepository
	{
        CustomerModel? GetById(int customerId);
        CustomerModel Insert(CustomerModel customer);
        CustomerModel Update(CustomerModel customer);
    }
}

