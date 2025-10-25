using Infra.RawQueryResult;

namespace Application.Messaging.Response.Agendamento
{
    public class AgendamentoOperacionalResponse
    {
        public List<AgendamentoOperacionalRawQuery> AgendamentoOperacionais {  get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
