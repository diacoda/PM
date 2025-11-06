using Xunit;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using PM.Infrastructure.Data;

namespace Integration.Tests
{
    public class PortfolioRepositoryIntegrationTests
    {
        [Fact]
        public async Task Should_Save_And_Retrieve_Portfolio()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseSqlite(Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ?? "DataSource=:memory:")
                .Options;

            using var context = new PortfolioDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            var portfolio = new Portfolio("Person1");
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var retrieved = await context.Portfolios.FirstAsync();
            retrieved.Owner.Should().Be("Person1");
        }
    }
}
