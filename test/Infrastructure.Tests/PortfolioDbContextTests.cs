using Xunit;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using PM.Infrastructure.Data;
using PM.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Integration.Tests
{
    public class PortfolioDbContextTests
    {
        [Fact]
        public async Task Should_Save_And_Retrieve_Portfolio()
        {
            var connString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ?? "DataSource=:memory:";
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseSqlite(connString)
                .Options;

            using var context = new PortfolioDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            var portfolio = new Portfolio("Integration Test");
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var retrieved = await context.Portfolios.FirstAsync();
            retrieved.Owner.Should().Be("Integration Test");
        }
    }
}
