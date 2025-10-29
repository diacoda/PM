namespace PM.DTO;

/// <summary>
/// Data Transfer Object used to create a new account in a portfolio.
/// </summary>
/// <param name="Name">The name of the account.</param>
/// <param name="Currency">The currency code for the account (e.g., "CAD", "USD").</param>
/// <param name="Institution">The financial institution where the account is held.</param>
public record CreateAccountDTO(string Name, string Currency, string Institution);
