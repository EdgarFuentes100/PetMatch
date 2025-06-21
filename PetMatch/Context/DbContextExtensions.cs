using Microsoft.EntityFrameworkCore;
using System.Data;

namespace PetMatch.Context
{
    public static class DbContextExtensions
    {
        public static async Task ExecuteInTransactionAsync(
            this DbContext db,
            Func<Task> operation,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken token = default)
        {
            await using var tx = await db.Database.BeginTransactionAsync(isolation, token);
            try
            {
                await operation();           // ← tu lógica de inserción/update/etc.
                await tx.CommitAsync(token);
            }
            catch
            {
                await tx.RollbackAsync(token);
                throw;
            }
        }
    }
}
