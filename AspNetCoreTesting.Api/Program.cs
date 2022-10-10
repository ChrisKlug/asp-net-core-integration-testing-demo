using AspNetCoreTesting.Api.Data;
using AspNetCoreTesting.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => {
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddDbContext<ApiContext>(x => {
    x.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});
builder.Services.AddScoped<IUsers, Users>();
builder.Services.AddSingleton<INotificationService, DummyNotificationService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization(config =>
{
    config.DefaultPolicy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{

}
