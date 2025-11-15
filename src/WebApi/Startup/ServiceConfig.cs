using Microsoft.Extensions.DependencyInjection;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Events;
using PM.SharedKernel;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register application service implementations for dependency injection.
    /// </summary>
    /// <remarks>
    /// Centralizes registration of all core business services, including account management, holdings,
    /// transactions, portfolios, valuations, pricing, FX, and workflow services.
    /// </remarks>
    public static class ServiceConfig
    {
        /// <summary>
        /// Registers application service implementations into the DI container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddApplicationServices();
        /// </code>
        /// </example>
        /// 
        /// 
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<SendNotificationOnTransactionAdded>();
            services.AddSingleton<IDomainEventPublisher>(sp =>
            {
                var pub = new InMemoryDomainEventPublisher();

                // register handlers here
                pub.Subscribe<TransactionAddedEvent>(async (evt, ct) =>
                {
                    using var scope = sp.CreateScope();
                    var scopedProvider = scope.ServiceProvider;
                    var handler = scopedProvider.GetRequiredService<SendNotificationOnTransactionAdded>();
                    await handler.Handle(evt, ct);
                });

                return pub;
            });
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IHoldingService, HoldingService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<IValuationService, ValuationService>();
            services.AddScoped<IValuationCalculator, ValuationCalculator>();
            services.AddScoped<ICashFlowService, CashFlowService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IFxRateService, FxRateService>();
            services.AddScoped<ITransactionWorkflowService, TransactionWorkflowService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IReportingService, ReportingService>();

            return services;
        }
    }
}
