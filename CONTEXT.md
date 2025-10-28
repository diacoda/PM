dotnet ef migrations add InitialCashFlows \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context CashFlowDbContext

dotnet ef migrations add Initial \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context AppDbContext

# ValuationDbContext

dotnet ef migrations add SOMETEXT \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context ValuationDbContext

dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj --context ValuationDbContext

dotnet ef database update --context ValuationDbContext --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj
