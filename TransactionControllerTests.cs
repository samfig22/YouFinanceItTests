using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YouFinanceIt.Data;
using YouFinanceIt.Services;
using YouFinanceIt.Models;

namespace YouFinanceIt.Tests
{
    public class TransactionControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsTransactionSuccessfully()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                TransactionID = 1,
                UserID = "user1",
                AccountID = 1,
                Amount = 100m,
                Description = "Test Add",
                TransactionDate = DateTime.UtcNow,
                CategoryID = 1,
                CreatedDate = DateTime.UtcNow
            };

            await service.AddAsync(transaction);

            var saved = await context.Transactions.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal("Test Add", saved.Description);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyUserTransactions()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            context.Transactions.AddRange(
                new Transaction
                {
                    TransactionID = 1,
                    UserID = "user1",
                    Amount = 50m,
                    Description = "Desc1",
                    AccountID = 1,
                    CategoryID = 1,
                    TransactionDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                },
                new Transaction
                {
                    TransactionID = 2,
                    UserID = "user2",
                    Amount = 75m,
                    Description = "Desc2",
                    AccountID = 1,
                    CategoryID = 1,
                    TransactionDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var results = await service.GetAllAsync("user1");

            Assert.Single(results);
            Assert.Equal(50m, results.First().Amount);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            context.Transactions.Add(new Transaction
            {
                TransactionID = 1,
                UserID = "user1",
                Amount = 25m,
                Description = "Desc",
                AccountID = 1,
                CategoryID = 1,
                TransactionDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var result = await service.GetByIdAsync(1, "user1");

            Assert.NotNull(result);
            Assert.Equal(25m, result.Amount);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTransactionWhenExists()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                TransactionID = 1,
                UserID = "user1",
                Amount = 30m,
                Description = "Original",
                AccountID = 1,
                CategoryID = 1,
                TransactionDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            transaction.Amount = 50m;
            transaction.Description = "Updated";

            await service.UpdateAsync(transaction);

            var updated = await context.Transactions.FindAsync(1);
            Assert.Equal(50m, updated.Amount);
            Assert.Equal("Updated", updated.Description);
        }

        [Fact]
        public async Task DeleteAsync_RemovesTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                TransactionID = 1,
                UserID = "user1",
                Amount = 20m,
                Description = "Desc",
                AccountID = 1,
                CategoryID = 1,
                TransactionDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            await service.DeleteAsync(1, "user1");

            var deleted = await context.Transactions.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
