using System.Net.Mail;
using QuoteMaster.Domain.Exceptions;

namespace QuoteMaster.Domain.Rules;

public static class ClientDataValidator
{
    public static void Validate(string name, string? email, string? document)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do cliente é obrigatório.");
        if (name.Trim().Length > 160) throw new DomainException("O nome do cliente deve ter no máximo 160 caracteres.");
        if (document?.Length > 18) throw new DomainException("O documento deve ter no máximo 18 caracteres.");
        if (email?.Length > 254) throw new DomainException("O e-mail deve ter no máximo 254 caracteres.");

        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                var parsed = new MailAddress(email.Trim());
                if (!string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new DomainException("O e-mail informado é inválido.");
            }
            catch (FormatException)
            {
                throw new DomainException("O e-mail informado é inválido.");
            }
        }

        if (!string.IsNullOrWhiteSpace(document) && !IsValidBrazilianDocument(document))
            throw new DomainException("Informe um CPF ou CNPJ válido.");
    }

    private static bool IsValidBrazilianDocument(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length == 11) return IsValidCpf(digits);
        if (digits.Length == 14) return IsValidCnpj(digits);
        return false;
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;
        var first = CalculateDigit(cpf[..9], [10, 9, 8, 7, 6, 5, 4, 3, 2]);
        var second = CalculateDigit(cpf[..9] + first, [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]);
        return cpf[9] - '0' == first && cpf[10] - '0' == second;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;
        var first = CalculateDigit(cnpj[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        var second = CalculateDigit(cnpj[..12] + first, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        return cnpj[12] - '0' == first && cnpj[13] - '0' == second;
    }

    private static int CalculateDigit(string value, int[] weights)
    {
        var remainder = value.Select((digit, index) => (digit - '0') * weights[index]).Sum() % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
