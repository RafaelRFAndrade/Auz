using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Agendamentos
{
    public interface IAgendamentoRepository
    {
        List<ObterAgendamentosRawQuery> ObterAgendamentosPorParceiro(Guid codigoParceiro, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizadorAgendamentosPorParceiro(Guid codigoParceiro);
        void Inserir(Agendamento agendamento);
        List<AgendamentoRawQueryResult> ObterAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial);
        CountRawQuery ObterQtdAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial);
        bool VerificarDisponibilidade(Guid codigoAtendimento, DateTime dataAgendamento);
        List<ObterAgendamentosPorAtendimentoRawQuery> ObterAgendamentosPorAtendimento(Guid codigoAtendimento);
        List<AgendamentoOperacionalRawQuery> ObterOperacional(Guid codigoUsuario, Guid codigoMedico, int pagina, int itensPorPagina);
        CountRawQuery ObterTotalizadorOperacional(Guid codigoUsuario, Guid codigoMedico);
        Agendamento Obter(Guid codigoAgendamento);
        void Atualizar(Agendamento agendamento);
    }
}
