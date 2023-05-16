using System;
using System.Linq.Expressions;

namespace ShipmentMS.Repositories
{
    /*
	 * https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
	 * 
	 */
    public interface IRepository<PK,T> : IDisposable
	{
        IEnumerable<T> Get(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "");

        T? GetById(PK id);

        void Insert(T value);

        void Delete(PK id);

        void Update(T newValue);

        void Save();
    }
}

