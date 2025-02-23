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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
