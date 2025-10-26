using Infra.RawQueryResult;

namespace Application.Messaging.Response.Agendamento
{
    public class DetalhadoAtendimentoResponse
    {
        public List<ObterAgendamentosPorAtendimentoRawQuery> Agendamentos { get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
