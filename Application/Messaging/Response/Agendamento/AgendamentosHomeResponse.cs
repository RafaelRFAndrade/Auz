using Infra.RawQueryResult;

namespace Application.Messaging.Response.Agendamento
{
    public class AgendamentosHomeResponse
    {
        public List<ObterAgendamentosRawQuery> Agendamentos {  get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
