using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Application.Abstractions;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Contracts.Common;
using LaundrySaas.Domain.MultiTenancy;
using LaundrySaas.Domain.Users;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Data;

namespace LaundrySaas.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants");

        group.MapPost("/register", RegisterTenantAsync)
            .WithName("RegisterTenant")
            .WithOpenApi();

        group.MapPost("/login", LoginTenantAsync)
            .WithName("LoginTenant")
            .WithOpenApi();

        group.MapPost("/login/firebase", LoginTenantFirebaseAsync)
            .WithName("LoginTenantFirebase")
            .WithOpenApi();

        group.MapPost("/complete-profile", CompleteProfileAsync)
            .WithName("CompleteProfile")
            .WithOpenApi();
    }

    private static async Task<IResult> RegisterTenantAsync(
        RegisterTenantRequest request,
        ApplicationDbContext db,
        ITokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName) ||
            string.IsNullOrWhiteSpace(request.TenantName) ||
            string.IsNullOrWhiteSpace(request.OwnerFullName) ||
            string.IsNullOrWhiteSpace(request.OwnerEmail) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.CountryCode) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Results.BadRequest(ApiResponse.CreateError("All fields are required.", "Missing fields."));
        }

        // Validate unique tenant name and user email globally
        var tenantExists = await db.Tenants.AnyAsync(t => t.Name.ToLower() == request.TenantName.ToLower());
        if (tenantExists)
        {
            return Results.BadRequest(ApiResponse.CreateError("Tenant name is already taken.", "TenantNameTaken"));
        }

        var emailExists = await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.ToLower() == request.OwnerEmail.ToLower());
        if (emailExists)
        {
            return Results.BadRequest(ApiResponse.CreateError("Email address is already registered.", "EmailRegistered"));
        }

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // 1. Create Tenant
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, request.TenantName, request.CompanyName, request.CountryCode, request.PhoneNumber);
            db.Tenants.Add(tenant);

            // 2. Initialize Credit Balance
            var creditBalance = new CreditBalance(Guid.NewGuid(), tenantId, "IDR");
            db.CreditBalances.Add(creditBalance);

            // 3. Create Owner User
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var userId = Guid.NewGuid();
            var user = new User(userId, tenantId, request.OwnerFullName, request.OwnerEmail, passwordHash, UserRole.Owner);
            db.Users.Add(user);

            // Save basic entity context first so relation constraints are satisfied
            await db.SaveChangesAsync();

            // 4. Seed Default Branch
            var branchId = Guid.NewGuid();
            var branch = new Branch(branchId, tenantId, "Central Branch", "Default Address", "-");
            db.Branches.Add(branch);

            // 5. Seed Menu Access for Owner
            var globalMenus = await db.MenuItems.ToListAsync();
            foreach (var menu in globalMenus)
            {
                var roleMenuAccess = new RoleMenuAccess(Guid.NewGuid(), tenantId, UserRole.Owner, menu.Id);
                db.RoleMenuAccesses.Add(roleMenuAccess);
            }

            // 6. Seed Default Payment Methods
            var cashMethod = new PaymentMethod(Guid.NewGuid(), tenantId, "Cash");
            var qrisMethod = new PaymentMethod(Guid.NewGuid(), tenantId, "QRIS");
            var tfMethod = new PaymentMethod(Guid.NewGuid(), tenantId, "Bank Transfer");
            db.PaymentMethods.AddRange(cashMethod, qrisMethod, tfMethod);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Generate JWT for the new owner user
            var token = tokenService.GenerateToken(user);
            var expiry = DateTime.UtcNow.AddHours(24); // Matches default token expiry claim configured

            var response = new AuthResponse(
                token,
                expiry,
                user.Id,
                user.Email,
                user.FullName,
                user.Role.ToString(),
                tenant.Id,
                tenant.Name
            );

            return Results.Ok(ApiResponse<AuthResponse>.CreateSuccess(response, "Tenant registered successfully."));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.BadRequest(ApiResponse.CreateError("Registration failed.", ex.Message));
        }
    }



    private static async Task<IResult> LoginTenantAsync(
        LoginRequest request,
        ApplicationDbContext db,
        ITokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(ApiResponse.CreateError("Email and password are required.", "MissingCredentials"));
        }

        // Bypassing tenant validation check to search for user globally
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !user.IsActive)
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "Invalid email or password."), statusCode: StatusCodes.Status401Unauthorized);
        }

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "Invalid email or password."), statusCode: StatusCodes.Status401Unauthorized);
        }

        // Find tenant info
        var tenant = await db.Tenants.FindAsync(user.TenantId);
        if (tenant == null || !tenant.IsActive)
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "Associated tenant is inactive or not found."), statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = tokenService.GenerateToken(user);
        var expiry = DateTime.UtcNow.AddHours(24);

        var response = new AuthResponse(
            token,
            expiry,
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            tenant.Id,
            tenant.Name
        );

        return Results.Ok(ApiResponse<AuthResponse>.CreateSuccess(response, "Login successful."));
    }

    private static async Task<IResult> LoginTenantFirebaseAsync(
        LoginFirebaseRequest request,
        ApplicationDbContext db,
        IFirebaseAuthService firebaseAuthService,
        ITokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Results.BadRequest(ApiResponse.CreateError("Firebase ID Token is required.", "MissingToken"));
        }

        FirebaseTokenPayload payload;
        try
        {
            payload = await firebaseAuthService.VerifyIdTokenAsync(request.IdToken);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse.CreateError("Firebase token verification failed.", ex.Message));
        }

        // Find user by Firebase UID or email globally
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.FirebaseUid == payload.Uid || u.Email.ToLower() == payload.Email.ToLower());

        if (user == null)
        {
            return Results.Json(ApiResponse.CreateError("User not registered.", "User not registered. Please register a tenant first."), statusCode: StatusCodes.Status404NotFound);
        }

        if (!user.IsActive)
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "User is inactive."), statusCode: StatusCodes.Status401Unauthorized);
        }

        // Link FirebaseUid and PhotoUrl if the user originally signed up via email/password
        bool profileUpdated = false;
        if (string.IsNullOrEmpty(user.FirebaseUid))
        {
            user.LinkFirebaseAccount(payload.Uid, payload.PictureUrl);
            profileUpdated = true;
        }
        else if (user.PhotoUrl != payload.PictureUrl)
        {
            user.UpdatePhotoUrl(payload.PictureUrl);
            profileUpdated = true;
        }

        if (profileUpdated)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
        }

        var tenant = await db.Tenants.FindAsync(user.TenantId);
        if (tenant == null || !tenant.IsActive)
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "Associated tenant is inactive or not found."), statusCode: StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrEmpty(tenant.CountryCode) || string.IsNullOrEmpty(tenant.PhoneNumber))
        {
            return Results.Ok(ApiResponse<object>.CreateSuccess(new
            {
                tenantId = tenant.Id,
                userId = user.Id,
                email = user.Email,
                isProfileComplete = false
            }, "Tenant profile is incomplete. Please complete your profile."));
        }

        var token = tokenService.GenerateToken(user);
        var expiry = DateTime.UtcNow.AddHours(24);

        var response = new AuthResponse(
            token,
            expiry,
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            tenant.Id,
            tenant.Name
        );

        return Results.Ok(ApiResponse<AuthResponse>.CreateSuccess(response, "Login successful."));
    }

    private static async Task<IResult> CompleteProfileAsync(
        CompleteProfileRequest request,
        ApplicationDbContext db,
        ITokenService tokenService)
    {
        if (request.UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.CountryCode) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Results.BadRequest(ApiResponse.CreateError("UserId, Password, CountryCode, and PhoneNumber are required.", "MissingFields"));
        }

        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
        {
            return Results.Json(ApiResponse.CreateError("User not found.", "Invalid UserId."), statusCode: StatusCodes.Status404NotFound);
        }

        if (!user.IsActive)
        {
            return Results.Json(ApiResponse.CreateError("Authentication failed.", "User is inactive."), statusCode: StatusCodes.Status401Unauthorized);
        }

        var tenant = await db.Tenants.FindAsync(user.TenantId);
        if (tenant == null || !tenant.IsActive)
        {
            return Results.BadRequest(ApiResponse.CreateError("Associated active tenant not found.", "TenantInactiveOrNotFound"));
        }

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // 1. Hash the new password and update user's profile
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.SetPassword(passwordHash);
            db.Users.Update(user);

            // 2. Update Tenant's CountryCode and PhoneNumber
            tenant.UpdateDetails(tenant.Name, tenant.CompanyName, request.CountryCode, request.PhoneNumber);
            db.Tenants.Update(tenant);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            // 3. Generate token and return full AuthResponse
            var token = tokenService.GenerateToken(user);
            var expiry = DateTime.UtcNow.AddHours(24);

            var response = new AuthResponse(
                token,
                expiry,
                user.Id,
                user.Email,
                user.FullName,
                user.Role.ToString(),
                tenant.Id,
                tenant.Name
            );

            return Results.Ok(ApiResponse<AuthResponse>.CreateSuccess(response, "Profile completed successfully."));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.BadRequest(ApiResponse.CreateError("Failed to complete profile.", ex.Message));
        }
    }
}
