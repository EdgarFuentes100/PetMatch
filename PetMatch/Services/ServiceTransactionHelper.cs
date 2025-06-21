using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using System.Data;

namespace PetMatch.Services
{
    public class ServiceTransactionHelper
    {
        private readonly AppDbContext _db;            
        public ServiceTransactionHelper(AppDbContext db)     // ← DI lo puede resolver
            => _db = db;

        public Task RunAsync(Func<Task> work,
                             IsolationLevel iso = IsolationLevel.ReadCommitted) =>
            _db.ExecuteInTransactionAsync(work, iso);
    }

}
