using PdfKnowledgeAssistant.Services;
using PdfKnowledgeAssistant.Services.Agent;
using PdfKnowledgeAssistant.Services.Employee;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<PdfExtractionService>();
builder.Services.AddSingleton<ChunkingService>();
builder.Services.AddScoped<VectorStorageService>();
builder.Services.AddScoped<SemanticSearchService>();

builder.Services.AddSingleton<ConversationMemory>();

builder.Services.AddScoped<EmployeeLeaveService>();
builder.Services.AddScoped<AgentOrchestrator>();

builder.Services.AddHttpClient<EmbeddingService>(
    client =>
    {
        client.BaseAddress =
            new Uri("http://localhost:11434/");
        client.Timeout = TimeSpan.FromMinutes(5);
    });

builder.Services.AddHttpClient<LlmService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
