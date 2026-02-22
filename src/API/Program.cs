using API.Extensions;
using Application;
using Infrastructure;
using Soliss.NuGetRepo.ApiCommonServices;
using System.Reflection;

var builder = SolissNetWebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.BuildWithSharedPipeline("/");

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapHealthChecks("/health")
    .RequireAuthorization();

app.Run();

// Required for functional and integration tests to work.
public partial class Program;
