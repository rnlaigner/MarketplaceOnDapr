using CustomerMS.Infra;
using CustomerMS.Models;

namespace CustomerMS.Repositories
{
	public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDbContext dbContext;

        public CustomerRepository(CustomerDbContext customerDbContext)
		{
            this.dbContext = customerDbContext;
        }

        public CustomerModel Insert(CustomerModel customer)
        {
            customer.created_at = DateTime.UtcNow;
            customer.updated_at = customer.created_at;
            var res = this.dbContext.Customers.Add(customer);
            this.dbContext.SaveChanges();
            return res.Entity;
        }

        public CustomerModel Update(CustomerModel customer)
        {
            customer.updated_at = DateTime.UtcNow;
            var res = this.dbContext.Customers.Update(customer);
            this.dbContext.SaveChanges();
            return res.Entity;
        }

        public CustomerModel? GetById(int id)
        {
            return this.dbContext.Customers.Find(id);
        }

    }
}

