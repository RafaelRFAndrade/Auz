using Infra.RawQueryResult;

namespace Application.Messaging.Response.Medico
{
    public class UsuariosVinculadosResponse
    {
        public List<UsuariosVinculadosRawQuery> UsuariosVinculados {  get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
