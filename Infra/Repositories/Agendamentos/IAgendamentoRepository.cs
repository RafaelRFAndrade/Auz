using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Agendamentos
{
    public interface IAgendamentoRepository
    {
        IEnumerable<ObterAgendamentosRawQuery> ObterAgendamentosPorCodigoUsuario(Guid codigoUsuario);
        void Inserir(Agendamento agendamento);
        List<AgendamentoRawQueryResult> ObterAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial);
        CountRawQuery ObterQtdAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial);
        bool VerificarDisponibilidade(Guid codigoAtendimento, DateTime dataAgendamento);
        List<ObterAgendamentosPorAtendimentoRawQuery> ObterAgendamentosPorAtendimento(Guid codigoAtendimento);
    }
}
