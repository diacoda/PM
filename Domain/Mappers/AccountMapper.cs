using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Domain.Mappers;

public static class AccountMapper
{
    public static Account ToEntity(CreateAccountDTO dto) =>
        new Account(
            dto.Name,
            new Currency(dto.Currency),
            Enum.Parse<Domain.Enums.FinancialInstitutions>(dto.Institution));

    public static AccountDTO ToDTO(Account account) => new AccountDTO
    {
        Id = account.Id,
        Name = account.Name,
        Currency = account.Currency.Code,
        FinancialInstitution = account.FinancialInstitution.ToString(),
        PortfolioId = account.PortfolioId
    };        
}
