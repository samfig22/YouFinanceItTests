// Tests/Services/TransactionServiceTests.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YouFinanceIt.Data;
using YouFinanceIt.Models;
using YouFinanceIt.Services;

namespace YouFinanceIt.Tests.Services
{
    public class TransactionServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                UserID = "user1",
                AccountID = 1,
                // CategoryID = 1,
                Description = "Test Transaction",
                Amount = 100,
                TransactionDate = DateTime.UtcNow
            };

            await service.AddAsync(transaction);
            var transactions = await service.GetAllAsync("user1");

            Assert.Single(transactions);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyUserTransactions()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            await service.AddAsync(new Transaction
            {
                UserID = "user1",
                AccountID = 1,
                // CategoryID = 1,
                Description = "User1 Transaction",
                Amount = 100,
                TransactionDate = DateTime.UtcNow
            });

            await service.AddAsync(new Transaction
            {
                UserID = "user2",
                AccountID = 2,
                // CategoryID = 2,
                Description = "User2 Transaction",
                Amount = 200,
                TransactionDate = DateTime.UtcNow
            });

            var user1Transactions = await service.GetAllAsync("user1");

            Assert.Single(user1Transactions);
            Assert.Equal("User1 Transaction", user1Transactions.First().Description);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                UserID = "user1",
                AccountID = 1,
                // CategoryID = 1,
                Description = "Specific Transaction",
                Amount = 50,
                TransactionDate = DateTime.UtcNow
            };

            await service.AddAsync(transaction);
            var result = await service.GetByIdAsync(transaction.TransactionID, "user1");

            Assert.NotNull(result);
            Assert.Equal("Specific Transaction", result.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                UserID = "user1",
                AccountID = 1,
                // CategoryID = 1,
                Description = "Before Update",
                Amount = 75,
                TransactionDate = DateTime.UtcNow
            };

            await service.AddAsync(transaction);

            transaction.Description = "After Update";
            await service.UpdateAsync(transaction);

            var result = await service.GetByIdAsync(transaction.TransactionID, "user1");
            Assert.Equal("After Update", result.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction()
        {
            using var context = GetInMemoryDbContext();
            var service = new TransactionService(context);

            var transaction = new Transaction
            {
                UserID = "user1",
                AccountID = 1,
                // CategoryID = 1,
                Description = "To Delete",
                Amount = 60,
                TransactionDate = DateTime.UtcNow
            };

            await service.AddAsync(transaction);
            await service.DeleteAsync(transaction.TransactionID, "user1");

            var result = await service.GetByIdAsync(transaction.TransactionID, "user1");
            Assert.Null(result);
        }
    }
}
