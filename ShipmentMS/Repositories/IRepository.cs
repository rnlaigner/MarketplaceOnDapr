using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ShipmentMS.Repositories;

/*
 * https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
 */
public interface IRepository<PK,T> : IDisposable
{

    T? GetById(PK id);

    void Insert(T value);

    void InsertAll(List<T> values);

    void Delete(PK id);

    void Update(T newValue);

    void Save();

    // API for ShipmentService
    IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot);

}


