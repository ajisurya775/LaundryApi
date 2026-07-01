using System.Threading.Tasks;

namespace LaundrySaas.Application.Abstractions;

public record FirebaseTokenPayload(
    string Uid,
    string Email,
    string? Name,
    string? PictureUrl
);

public interface IFirebaseAuthService
{
    Task<FirebaseTokenPayload> VerifyIdTokenAsync(string idToken);
}
