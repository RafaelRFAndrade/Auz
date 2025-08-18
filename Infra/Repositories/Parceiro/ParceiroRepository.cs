using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Parceiro
{
    public class ParceiroRepository : RepositoryBase, IParceiroRepository
    {
        public ParceiroRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Domain.Entidades.Parceiro parceiro)
        {
            Add(parceiro);
            SaveChanges();
        }
    }
}
