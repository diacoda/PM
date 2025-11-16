using Microsoft.Extensions.DependencyInjection;
using PM.Application.Commands;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Events;
using PM.InMemoryEventBus;
using PM.SharedKernel.Events;

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
            /*
            services.AddScoped<PM.InMemoryEventBus.IEventDispatcher, EventDispatcher>();
            services.AddScoped<TransactionAddedHandler>();
            services.AddScoped<Event<TransactionAddedHandler>>();
            services.AddSingleton<IEventBus>(sp =>
            {
                var bus = new PM.InMemoryEventBus.InMemoryEventBus(sp.GetRequiredService<IServiceScopeFactory>());
                bus.Subscribe<TransactionAddedEvent>(sp => sp.GetRequiredService<TransactionAddedHandler>());
                return bus;
            });
            */

            services.AddScoped<PM.InMemoryEventBus.IEventDispatcher, EventDispatcher>();
            services.AddScoped<TransactionAddedHandler>();
            services.AddSingleton<IEventBus>(sp =>
            {
                var bus = new PM.InMemoryEventBus.InMemoryEventBus(sp.GetRequiredService<IServiceScopeFactory>());

                // Subscribe with wrapper to handle Event<T> with metadata
                bus.Subscribe<Event<TransactionAddedEvent>>(sp =>
                    new EventHandlerWrapper<TransactionAddedEvent>(
                        sp.GetRequiredService<TransactionAddedHandler>()
                    )
                );
                return bus;
            });

            services.AddScoped<EventDispatcher>();

            services.AddScoped<PM.SharedKernel.Events.IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<SendNotificationOnTransactionAdded>();
            services.AddScoped<DailyPricesFetchedEventHandler>();

            services.AddSingleton<IDomainEventPublisher>(sp =>
            {
                var publisher = new InMemoryDomainEventPublisher(
                    sp.GetRequiredService<IServiceScopeFactory>());

                // Manual subscription
                publisher.Register<TransactionAddedEvent, SendNotificationOnTransactionAdded>();
                publisher.Register<DailyPricesFetchedEvent, DailyPricesFetchedEventHandler>();

                return publisher;
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
