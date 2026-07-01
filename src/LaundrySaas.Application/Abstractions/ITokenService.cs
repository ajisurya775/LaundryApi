using LaundrySaas.Domain.Users;

namespace LaundrySaas.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user);
}
