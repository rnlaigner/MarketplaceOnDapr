using CustomerMS.Infra;
using CustomerMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerMS.Repositories;

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

    public void Cleanup()
    {
        this.dbContext.Customers.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void Reset()
    {
        this.dbContext.Database.ExecuteSqlRaw("UPDATE customers SET delivery_count=0, failed_payment_count=0, success_payment_count=0");
        this.dbContext.SaveChanges();
    }

}

