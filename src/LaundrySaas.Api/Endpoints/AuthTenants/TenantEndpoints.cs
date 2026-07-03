using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LaundrySaas.Application.Common;
using LaundrySaas.Application.Contracts.Auth;
using LaundrySaas.Application.Identity.Register;
using LaundrySaas.Application.Identity.Login;
using LaundrySaas.Application.Identity.CompleteProfile;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Api.Endpoints.AuthTenants;

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
        ICommandHandler<RegisterTenantCommand, ApiResponse<AuthResponse>> handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterTenantCommand(
            request.TenantName,
            request.CompanyName,
            request.OwnerFullName,
            request.OwnerEmail,
            request.Password,
            request.CountryCode,
            request.PhoneNumber);

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> LoginTenantAsync(
        LoginRequest request,
        ICommandHandler<LoginCommand, ApiResponse<AuthResponse>> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await handler.HandleAsync(command, cancellationToken);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> LoginTenantFirebaseAsync(
        LoginFirebaseRequest request,
        ICommandHandler<LoginFirebaseCommand, ApiResponse<object>> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginFirebaseCommand(request.IdToken);
        var result = await handler.HandleAsync(command, cancellationToken);

        if (!result.Success)
        {
            if (result.Message == "User not registered. Please register a tenant first.")
            {
                return Results.Json(result, statusCode: StatusCodes.Status404NotFound);
            }
            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CompleteProfileAsync(
        CompleteProfileRequest request,
        ICommandHandler<CompleteProfileCommand, ApiResponse<AuthResponse>> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteProfileCommand(
            request.UserId,
            request.Password,
            request.CountryCode,
            request.PhoneNumber);

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}

public record RegisterTenantRequest(
    string TenantName,
    string CompanyName,
    string OwnerFullName,
    string OwnerEmail,
    string Password,
    string CountryCode,
    string PhoneNumber);

public record LoginRequest(string Email, string Password);

public record LoginFirebaseRequest(string IdToken);

public record CompleteProfileRequest(
    Guid UserId,
    string Password,
    string CountryCode,
    string PhoneNumber);
