# CashFlowDbContext

dotnet ef migrations add SOMETEXT \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context CashFlowDbContext

dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj --context CashFlowDbContext

dotnet ef database update --context CashFlowDbContext --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj

# PortfolioDbContext

dotnet ef migrations add SOMETEXT \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context PortfolioDbContext

dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj --context PortfolioDbContext

dotnet ef database update --context PortfolioDbContext --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj

# ValuationDbContext

dotnet ef migrations add Valuations \
 --project Infrastructure/Infrastructure.csproj \
 --startup-project WebApi/WebApi.csproj \
 --context ValuationDbContext

dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj --context ValuationDbContext

dotnet ef database update --context ValuationDbContext --project Infrastructure/Infrastructure.csproj --startup-project WebApi/WebApi.csproj
