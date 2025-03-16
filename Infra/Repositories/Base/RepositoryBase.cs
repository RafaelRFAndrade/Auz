using Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Base
{
    public class RepositoryBase : DbContext
    {
        public RepositoryBase(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Paciente> Paciente { get; set; }
        public DbSet<Medico> Medico { get; set; }
        public DbSet<Atendimento> Atendimento { get; set; }
        public DbSet<Agendamento> Agendamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
