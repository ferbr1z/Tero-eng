using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Data;
using MbyteWeb.Server.Middlewares;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.ViewModels.Auth;
using Backend.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public class AuthFoundationTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task DefaultAdminCanLoginWithAllBasePermissions()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            telefono = "0986433663",
            password = "1234"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<LoginResponse>();
        session!.Token.Should().NotBeNullOrEmpty();
        session.Permisos.Should().BeEquivalentTo(PermisosEnum.GetPermisos());
    }

    [Fact]
    public async Task SeededRolesUseGeneratedIntegerIds()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ids = await context.Roles.Select(role => role.Id).ToListAsync();

        ids.Should().HaveCount(2).And.OnlyContain(id => id > 0);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task PermissionChangeRenewsAnExistingSession()
    {
        var phone = $"09{Random.Shared.Next(10000000, 99999999)}";
        int userId;
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            var user = new Usuario { UserName = phone, PhoneNumber = phone, Telefono = phone, Nombre = "Test", NumDocumento = string.Empty };
            (await userManager.CreateAsync(user, "Test1234")).Succeeded.Should().BeTrue();
            userId = user.Id;
        }

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { telefono = phone, password = "Test1234" });
        var session = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.UsuarioPermisos.Add(new UsuarioPermiso
            {
                UsuarioId = userId,
                PermisoId = PermisosEnum.Auditoria,
                HasPermiso = true
            });
            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/auth/session");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.Contains(SessionVersionMiddleware.RenewedHeader).Should().BeTrue();
        var renewed = await response.Content.ReadFromJsonAsync<LoginResponse>();
        renewed!.Permisos.Should().Contain(PermisosEnum.Auditoria);
    }
}
