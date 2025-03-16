using Application.Interfaces;
using Application.Services;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Repos
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IAtendimentoRepository, AtendimentoRepository>();
builder.Services.AddTransient<IPacienteRepository, PacienteRepository>();
builder.Services.AddTransient<IMedicoRepository, MedicoRepository>();
builder.Services.AddTransient<IAgendamentoRepository, AgendamentoRepository>();

//Services
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
builder.Services.AddTransient<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddTransient<IAtendimentoService, AtendimentoService>();
builder.Services.AddTransient<IPacienteService, PacienteService>();
builder.Services.AddTransient<IMedicoService, MedicoService>();
builder.Services.AddTransient<IAgendamentoService, AgendamentoService>();

builder.Services.AddDbContext<RepositoryBase>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
