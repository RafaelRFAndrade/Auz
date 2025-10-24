using Infra.RawQueryResult;

namespace Application.Messaging.Response.Usuario
{
    public class ObterMedicoUsuarioResponse
    {
        public List<ObterMedicoUsuarioRawQuery> MedicoUsuario {  get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}

