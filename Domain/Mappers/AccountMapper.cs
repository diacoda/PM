using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Mappers;

public static class AccountMapper
{
    public static Account ToEntity(CreateAccountDTO dto) =>
        new Account(
            dto.Name,
            new Currency(dto.Currency),
            Enum.Parse<Domain.Enums.FinancialInstitutions>(dto.FinancialInstitution));

    public static Account ToEntity(AccountDTO dto) =>
    new Account(
        dto.Name,
        new Currency(dto.Currency),
        Enum.Parse<Domain.Enums.FinancialInstitutions>(dto.FinancialInstitution));
    public static AccountDTO ToDTO(Account account) => new AccountDTO
    {
        Id = account.Id,
        Name = account.Name,
        Currency = account.Currency.Code,
        FinancialInstitution = account.FinancialInstitution.ToString(),
        PortfolioId = account.PortfolioId
    };

    public static AccountDTO ToDTO(Account account, IncludeOption[] includes)
    {
        var dto = new AccountDTO
        {
            Id = account.Id,
            Name = account.Name,
            Currency = account.Currency.Code,
            FinancialInstitution = account.FinancialInstitution.ToString(),
            PortfolioId = account.PortfolioId,
            Holdings = includes.Contains(IncludeOption.Accounts)
                ? account.Holdings.Select(HoldingMapper.ToDTO).ToList()
                : new List<HoldingDTO>(),
            /*Transactions = includes.Contains("transactions")
                ? account.Transactions.Select(TransactionMapper.ToDTO).ToList()
                : new List<TransactionDTO>()*/
        };

        return dto;
    }
}
