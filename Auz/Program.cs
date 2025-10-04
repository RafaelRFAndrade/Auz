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

var builder = WebApplication.CreateBuilder(args);

TimeZoneInfo.ClearCachedData();
var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

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

    // Configuração para permitir JWT via Swagger
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
});

//Repos
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IAtendimentoRepository, AtendimentoRepository>();
builder.Services.AddTransient<IPacienteRepository, PacienteRepository>();
builder.Services.AddTransient<IMedicoRepository, MedicoRepository>();
builder.Services.AddTransient<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddTransient<IParceiroRepository, ParceiroRepository>();
builder.Services.AddTransient<IDocumentoRepository, DocumentoRepository>();

//Services
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddTransient<IAtendimentoService, AtendimentoService>();
builder.Services.AddTransient<IPacienteService, PacienteService>();
builder.Services.AddTransient<IMedicoService, MedicoService>();
builder.Services.AddTransient<IAgendamentoService, AgendamentoService>();
builder.Services.AddTransient<IAwsService, AwsService>();

builder.Services.AddDbContext<RepositoryBase>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddAutoMapper(config => {
    config.CreateMap<AtualizarMedicoRequest, Medico>();
        //.ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
        //    srcMember != null)); // Só atualiza propriedades não nulas
    config.CreateMap<AtualizarPacienteRequest, Paciente>();
});

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
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
           };
       });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});



builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseHttpsRedirection();

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