using Infra.RawQueryResult;

namespace Application.Messaging.Response.Usuario
{
    public class ValoresHomeResponse : ResponseBase
    {
        public string NomeUsuario { get; set; }
        public int? QtdUsuarios { get; set; }
        public int? QtdOperadores { get; set; }
    }
}
