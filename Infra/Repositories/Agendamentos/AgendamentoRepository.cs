using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Agendamentos
{
    public class AgendamentoRepository : RepositoryBase, IAgendamentoRepository
    {
        public AgendamentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Agendamento agendamento)
        {
            Add(agendamento);
            SaveChanges();
        }
    }
}
