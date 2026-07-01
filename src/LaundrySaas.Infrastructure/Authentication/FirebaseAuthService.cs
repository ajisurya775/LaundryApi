using System;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using LaundrySaas.Application.Abstractions;

namespace LaundrySaas.Infrastructure.Authentication;

public class FirebaseAuthService : IFirebaseAuthService
{
    public FirebaseAuthService(IConfiguration configuration)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialPath = configuration["Firebase:CredentialFilePath"] 
                                 ?? "Secrets/laundry-firebase-private-key.json";

            // If path is relative, resolve it relative to current app directory
            if (!Path.IsPathRooted(credentialPath))
            {
                credentialPath = Path.Combine(AppContext.BaseDirectory, credentialPath);
            }

            if (!File.Exists(credentialPath))
            {
                throw new FileNotFoundException($"Firebase credential file not found at: {credentialPath}");
            }

#pragma warning disable CS0618
            using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromStream(stream)
            });
#pragma warning restore CS0618
        }
    }

    public async Task<FirebaseTokenPayload> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            
            var uid = decodedToken.Uid;
            
            // Extract email, name, and picture
            var email = decodedToken.Claims.TryGetValue("email", out var emailObj) 
                ? emailObj?.ToString() ?? string.Empty 
                : string.Empty;
                
            var name = decodedToken.Claims.TryGetValue("name", out var nameObj) 
                ? nameObj?.ToString() 
                : null;
                
            var picture = decodedToken.Claims.TryGetValue("picture", out var pictureObj) 
                ? pictureObj?.ToString() 
                : null;

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email claim is missing in the Firebase token.");
            }

            return new FirebaseTokenPayload(uid, email, name, picture);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Verification of Firebase ID Token failed.", ex);
        }
    }
}
