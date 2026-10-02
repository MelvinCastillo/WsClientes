using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WsSofterpRD.Data;
using WsSofterpRD.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Authorization;
using System;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT Key no está configurada en appsettings.json");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,              // 🔥 valida expiración
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(key),

        ClockSkew = TimeSpan.Zero             // 🔥 MUY IMPORTANTE
    };

    // 🔥 OPCIONAL: manejar eventos (debug / control)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }

            return Task.CompletedTask;
        },

  
        OnChallenge = context =>
        {
            // 🔥 permitir endpoints anónimos
            var endpoint = context.HttpContext.GetEndpoint();

            if (endpoint?.Metadata
                ?.GetMetadata<IAllowAnonymous>() != null)
            {
                return Task.CompletedTask;
            }

            context.HandleResponse();

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var message =
                context.AuthenticateFailure
                    is SecurityTokenExpiredException
                ? "Token expirado"
                : "No autorizado";

            return context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = message
                })
            );
        }
    };
});
builder.Services.AddAuthorization();

// Usa configuración desde appsettings.json
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443; // o el puerto que uses
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            /*policy.WithOrigins("http://localhost:4200")  // frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();*/
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddDbContext<WsSofterpRDContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WsSofterpRDContext")));
// Habilita controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clientes API",
        Version = "v1"
    });

    // 🔐 CONFIGURAR JWT EN SWAGGER
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token así: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddMemoryCache();
var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine("AUTH HEADER: " + context.Request.Headers.Authorization);

    await next();

    Console.WriteLine("STATUS: " + context.Response.StatusCode);
});

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();    
app.UseAuthorization();
app.MapControllers();

 
app.UseSwagger();
app.UseSwaggerUI();

#region CientesWS
//Despliega la lista de Clientes
app.MapGet("/api/CientesWS/ClientesWS",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (
    HttpContext http,
   /* [FromQuery] int Idcodigo,
    [FromQuery] string Nombres1,
    [FromQuery] string Nombres2,
    [FromQuery] string Apellido1,
    [FromQuery] string Apellido2,
    [FromQuery] DateTime Fechacreacion,
    [FromQuery] int iduser,*/
    IConfiguration config) =>
{ 

    using var connection = new SqlConnection(
        config.GetConnectionString("WsSofterpRDContext"));

    await connection.OpenAsync();

    using var command = new SqlCommand("dbo.ClientesWS", connection);
    command.CommandType = CommandType.TableDirect;

 
    using var reader = await command.ExecuteReaderAsync();

    var dt = new DataTable();
    dt.Load(reader);

    var props = typeof(ClientesWS).GetProperties().ToList();

    var resultados = dt.AsEnumerable()
        .Select(row =>
        {
            var item = new ClientesWS();

            props
                .Where(prop => dt.Columns.Contains(prop.Name) && !row.IsNull(prop.Name))
                .ToList()
                .ForEach(prop =>
                {
                    var value = row[prop.Name];
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    try
                    {
                        prop.SetValue(item, Convert.ChangeType(value, targetType));
                    }
                    catch { }
                });

            return item;
        }).ToList();

    return Results.Ok(resultados);
})
.WithName("GetClientesWS")
.WithTags("ClientesWS");
 
#endregion



app.Run();