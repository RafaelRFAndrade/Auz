using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Atendimentos
{
    public class AtendimentoRepository : RepositoryBase, IAtendimentoRepository
    {
        public AtendimentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Atendimento atendimento)
        {
            Add(atendimento);
            SaveChanges();  
        }
    }
}
