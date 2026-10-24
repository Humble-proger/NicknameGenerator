using NicknameGenerator.Abstractions;
using NicknameGenerator.DependencyInjection;

WebApplicationBuilder _builder = WebApplication.CreateBuilder(args);

// Bind from appsettings.json -> "Nickname" section.
// Registered as singleton: options are frozen at startup (see README).
_builder.Services.AddNicknameGenerator(_builder.Configuration);

WebApplication _app = _builder.Build();

_app.MapGet("/", () => "NicknameGenerator sample. Try /api/nickname/{guid}");

_app.MapGet("/api/nickname/{userId:guid}", (Guid userId, INicknameGenerator generator) =>
	Results.Ok(new { userId, nickname = generator.Generate(userId) }));

// Retry endpoint - shows attempt-based collision handling.
_app.MapGet("/api/nickname/{userId:guid}/attempt/{attempt:int}", (Guid userId, int attempt, IRetryableNicknameGenerator generator) =>
	Results.Ok(new { userId, attempt, nickname = generator.Generate(userId, attempt) }));

_app.Run();

// Required for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
