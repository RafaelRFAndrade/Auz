using Infra.RawQueryResult;

namespace Application.Messaging.Response.Usuario
{
    public class ObterUsuariosParceiroResponse : ResponseBase
    {
        public List<UsuariosRawQuery> Usuarios { get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
