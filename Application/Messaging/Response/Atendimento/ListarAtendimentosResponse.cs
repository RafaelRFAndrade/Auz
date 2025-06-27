using Infra.RawQueryResult;

namespace Application.Messaging.Response.Atendimento
{
    public class ListarAtendimentosResponse
    {
        public List<ListarAtendimentosRawQuery> Atendimentos { get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
