using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Domain.Rules;

namespace QuoteMaster.UnitTests;

public sealed class ClientDataValidatorTests
{
    [Fact]
    public void AcceptsValidCpfAndCnpj()
    {
        ClientDataValidator.Validate("Pessoa", "pessoa@example.com", "529.982.247-25");
        ClientDataValidator.Validate("Empresa", null, "11.222.333/0001-81");
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("Pessoa", "e-mail-invalido", null)]
    [InlineData("Pessoa", null, "111.111.111-11")]
    public void RejectsInvalidClientData(string name, string? email, string? document)
    {
        Assert.Throws<DomainException>(() => ClientDataValidator.Validate(name, email, document));
    }
}
