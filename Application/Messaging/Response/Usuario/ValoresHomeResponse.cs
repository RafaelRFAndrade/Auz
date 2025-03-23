using Infra.RawQueryResult;

namespace Application.Messaging.Response.Usuario
{
    public class ValoresHomeResponse : ResponseBase
    {
        public string NomeUsuario { get; set; }
        public IEnumerable<ObterAtendimentosRawQuery> Atendimentos { get; set; }
        public IEnumerable<ObterAgendamentosRawQuery> Agendamentos {  get; set; }
    }
}
