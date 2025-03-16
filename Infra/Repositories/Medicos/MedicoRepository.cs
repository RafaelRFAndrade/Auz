using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Medicos
{
    public class MedicoRepository : RepositoryBase, IMedicoRepository
    {
        public MedicoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Medico medico)
        {
            Add(medico);
            SaveChanges();
        }
    }
}
