using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NHibernate;
using RepoDb;
using System.Data;
using System.Security.Claims;
using System.Text;
using TODO_List.API.Filter;
using TODO_List.API.GraphQL;
using TODO_List.Application.DependencyInjection;
using TODO_List.Infrastructure.NhibernateConfig;
using TODO_List.Infrastructure.Storage;
using TODO_List.API.Hubs;
//using TODO_List.Infrastructure.Services;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Azure.Cosmos;
using TODO_List.Domain.Middlewares;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Email;
using System.Net;
using System.Net.Mail;
using Serilog.Formatting.Display;
using Serilog.Events;
using TODO_List.Alerts;
using Npgsql;

// ----------------------------
// Email configuration
// ----------------------------
var emailOptions = new EmailSinkOptions
{
    From = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "dnet25822@gmail.com",
    To = new List<string> { Environment.GetEnvironmentVariable("EMAIL_TO") ?? "hammad.ar999@gmail.com" },
    Host = "smtp.gmail.com",
    Port = 587,
    Credentials = new NetworkCredential
    (
        Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "dnet25822@gmail.com",
        Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "bbtxrkdmpxwulkxm"
    ),
    IsBodyHtml = false,
    Subject = new MessageTemplateTextFormatter
    (
        "🚨 TODO API ALERT 🚨",
        null
    ),
    Body = new MessageTemplateTextFormatter
    (
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
        null
    ),
    ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
};

// ----------------------------
// Telegram Credentials
// ----------------------------
string telegramWorkerUrl = "https://telegram-proxy-new.hammad-arshad2k25.workers.dev";
string telegramApiKey = Environment.GetEnvironmentVariable("TELEGRAM_WORKER_KEY")!;

// ----------------------------
// Serilog configuration
// ----------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://localhost:9200"))
    //{
    //    AutoRegisterTemplate = true,
    //    IndexFormat = "todolist-api-logs-{0:yyyy.MM.dd}",
    //    TypeName = null,
    //    MinimumLogEventLevel = LogEventLevel.Information,
    //    ModifyConnectionSettings = conn =>
    //        conn.BasicAuthentication(Environment.GetEnvironmentVariable("ELASTIC_USER") ?? "elastic",
    //                                 Environment.GetEnvironmentVariable("ELASTIC_PASSWORD") ?? "password")
    //            .ServerCertificateValidationCallback((o, c, ch, e) => true)
    //})
    .WriteTo.Email(
        emailOptions,
        restrictedToMinimumLevel: LogEventLevel.Error
    ).WriteTo.Sink(new TelegramSink(telegramWorkerUrl, telegramApiKey)).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Initialize RepoDb for SQL Server
//GlobalConfiguration.Setup().UseSqlServer();

// --------------------------------------------------
// FASTENDPOINTS (FE + Controllers)
// --------------------------------------------------
builder.Services.AddFastEndpoints().AddValidation();  // FE main services
builder.Services.SwaggerDocument(options =>
{
    options.DocumentSettings = s =>
    {
        s.Title = "TODO FastEndPoints API";
        s.Version = "v1";
        s.AddAuth("Bearer", new()
        {
            Name = "Authorization",
            In = NSwag.OpenApiSecurityApiKeyLocation.Header,
            Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
            Description = "Enter: Bearer {Paste Token Here}"
        });
    };
});
builder.Services.AddEndpointsApiExplorer();

// --------------------------------------------------
// CONTROLLERS + SWAGGERGEN (Swashbuckle)
// --------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TODO_List API (Controllers)",
        Version = "v1",
    });

    // JWT Support
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// --------------------------------------------------
// Output Cache + Response Cache
// --------------------------------------------------
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});
builder.Services.AddResponseCaching();

// --------------------------------------------------
// postgresql DATABASE
// --------------------------------------------------
// Step 1: Get the raw URL from Railway
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// Step 2: Parse it
var uri = new Uri(databaseUrl);
var userInfo = uri.UserInfo.Split(':');

// Step 3: Convert to Npgsql connection string
var connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};Pooling=true;SSL Mode=Require;Trust Server Certificate=true";

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure();
        npgsql.CommandTimeout(30);
    }));

// Enable IDbConnection injection (for Dapper)
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

// NHibernate
builder.Services.AddSingleton<ISessionFactory>(provider => NH_Helper.fact);
builder.Services.AddScoped<NHibernate.ISession>(provider =>
{
    var sf = provider.GetService<ISessionFactory>();
    var interceptor = new AuditInterceptors();
    var session = sf.WithOptions().Interceptor(interceptor).OpenSession();
    interceptor.SetSession(session);
    return session;
});

// --------------------------------------------------
// DEPENDENCY INJECTION
// --------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
//builder.Services.AddSingleton<ElasticService>();

// --------------------------------------------------
// GLOBAL FILTERS FOR CONTROLLERS
// --------------------------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ActionLogFilter>();
    options.Filters.Add<ExceptionLogFilter>();
});

// --------------------------------------------------
// GRAPHQL
// --------------------------------------------------
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .AddFiltering()
    .AddSorting();

// --------------------------------------------------
// JWT AUTH
// --------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwt["Issuer"],
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? jwt["Key"])),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

// --------------------------------------------------
// SignalR
// --------------------------------------------------
builder.Services.AddSignalR();
//builder.Services.AddSignalR().AddStackExchangeRedis(Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379", options =>
//{
//    options.Configuration.ChannelPrefix = "TodoList";      
//});

// --------------------------------------------------
// Aws Credentials for DynamoDB
// --------------------------------------------------
////builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
//{
    // LOCAL OR TEST DYNAMO (COMMENTED FOR RAILWAY)
    // var config = new AmazonDynamoDBConfig
    // {
    //     ServiceURL = "http://localhost:8000",
    //     UseHttp = true,
    //     AuthenticationRegion = "us-east-1"
    // };
    // return new AmazonDynamoDBClient(new BasicAWSCredentials("dummy", "dummy"), config);

    // PRODUCTION RAILWAY ENV VARS CAN GO HERE
    ////return null!;
//});

//builder.Services.AddScoped<IDynamoDBContext>(provider =>
//{
//    var client = provider.GetRequiredService<IAmazonDynamoDB>();
//    var config = new DynamoDBContextConfig { DisableFetchingTableMetadata = true };
//    return new DynamoDBContext(client, config);
//});

// --------------------------------------------------
// Azure Credentials for CosmosDB
// --------------------------------------------------
//builder.Services.AddSingleton(s =>
//{
//    string endpointuri = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT") ?? "https://localhost:8081";
//    string primarykey = Environment.GetEnvironmentVariable("COSMOS_KEY") ?? "your-primary-key";
//    return new CosmosClient(endpointuri, primarykey);
//});

var app = builder.Build();

// --------------------------------------------------
// PIPELINE
// --------------------------------------------------
if(!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLogingMiddleware>();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.UseFastEndpoints(c =>
{
    c.Errors.ResponseBuilder = (failures, ctx, statuscode) =>
    {
        return new
        {
            statuscode,
            message = "Validation Failed",
            errors = failures.Select(f => new { f.PropertyName, f.ErrorMessage })
        };
    };
});

app.UseSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TODO_List API V1 (Controllers)");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.MapGraphQL("/graphql");

app.MapGet("/ping", () => "Welcome to TODO API!");
app.MapGet("/testdb", async (TodoDbContext db) =>
{
    await db.Database.OpenConnectionAsync();
    return Results.Ok("Database OK");
});
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureCreated(); 
}

//app.UseStaticFiles();
//app.MapHub<TaskHub>("/taskHub");

// ----------------------------
// Dynamic port support for Railway
// ----------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

Serilog.Debugging.SelfLog.Enable(Console.Error);

app.Run();