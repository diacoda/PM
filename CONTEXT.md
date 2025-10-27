dotnet ef migrations add InitialCashFlows \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context CashFlowDbContext

dotnet ef migrations add Initial \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context AppDbContext

dotnet ef migrations add InitialValuations \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context ValuationDbContext
