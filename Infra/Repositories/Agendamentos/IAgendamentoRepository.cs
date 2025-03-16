using Domain.Entidades;

namespace Infra.Repositories.Agendamentos
{
    public interface IAgendamentoRepository
    {
        void Inserir(Agendamento agendamento);
    }
}
