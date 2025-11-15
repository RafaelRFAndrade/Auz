using Application.Interfaces;
using Application.Messaging.Request.Medico;
using Application.Messaging.Request.Paciente;
using Application.Services;
using Domain.Entidades;
using Infra.Repositories.Agendamentos;
using Infra.Repositories.Atendimentos;
using Infra.Repositories.Base;
using Infra.Repositories.Medicos;
using Infra.Repositories.Pacientes;
using Infra.Repositories.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Infra.Repositories.Parceiro;
using System.Text.Json;
using System.Text.Json.Serialization;
using Infra.Repositories.Documentos;
using Infra.RawQueryResult;
using Application.Messaging.Response.Atendimento;
using Microsoft.AspNetCore.Http.Features;
using Application.Messaging.Request.Usuario;
using Application.Messaging.Request.Parceiro;
using Infra.Repositories.MedicoUsuarioOperacional;
using Application.Messaging.Request.Agendamento;
using Auz.Extensions;
using Auz.Middleware;

var builder = WebApplication.CreateBuilder(args);

TimeZoneInfo.ClearCachedData();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auz",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite 'Bearer {seu token}' para autenticar"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    c.OperationFilter<FileUploadOperationFilter>();
});

//Repos
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IAtendimentoRepository, AtendimentoRepository>();
builder.Services.AddTransient<IPacienteRepository, PacienteRepository>();
builder.Services.AddTransient<IMedicoRepository, MedicoRepository>();
builder.Services.AddTransient<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddTransient<IParceiroRepository, ParceiroRepository>();
builder.Services.AddTransient<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddTransient<IMedicoUsuarioOperacionalRepository, MedicoUsuarioOperacionalRepository>();   

//Services
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddTransient<IAtendimentoService, AtendimentoService>();
builder.Services.AddTransient<IPacienteService, PacienteService>();
builder.Services.AddTransient<IMedicoService, MedicoService>();
builder.Services.AddTransient<IAgendamentoService, AgendamentoService>();
builder.Services.AddTransient<IAwsService, AwsService>();
builder.Services.AddTransient<IDocumentoService, DocumentoService>();
builder.Services.AddTransient<IParceiroService, ParceiroService>();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
if (connectionString?.Contains("#{") == true)
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string não configurada. Configure a variável de ambiente CONNECTION_STRING ou ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<RepositoryBase>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddAutoMapper(config => {
    config.CreateMap<AtualizarMedicoRequest, Medico>();
    config.CreateMap<AtualizarPacienteRequest, Paciente>();
    config.CreateMap<ObterAtendimentoRawQuery, ObterAtendimentoResponse>();
    config.CreateMap<AtualizarCompletoRequest, Medico>();
    config.CreateMap<AtualizarPacienteDetalhadoRequest, Paciente>();
    config.CreateMap<AtualizarUsuarioRequest, Usuario>();
    config.CreateMap<AtualizarParceiroRequest, Parceiro>();
    config.CreateMap<AtualizarDetalhadoRequest, Agendamento>();
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
    options.ValueLengthLimit = 104857600; 
    options.MemoryBufferThreshold = 2097152;
    options.MultipartBoundaryLengthLimit = 128;
    options.MultipartHeadersCountLimit = 16;
    options.MultipartHeadersLengthLimit = 16384;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; 
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
        ?? builder.Configuration["Jwt:Key"];
    
    if (jwtKey?.Contains("#{") == true)
    {
        jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? Environment.GetEnvironmentVariable("Jwt__Key");
    }
    
    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("JWT Key não configurada. Configure a variável de ambiente JWT_KEY ou Jwt__Key.");
    }
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "http://189.126.105.186:3000", "http://189.126.105.186:3001")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddAuthorization();

builder.Services.AddLokiLogging(builder.Configuration.GetSection("Logging:Loki"));

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (DateTime.TryParse(value, out var date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Local);
        }
        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}