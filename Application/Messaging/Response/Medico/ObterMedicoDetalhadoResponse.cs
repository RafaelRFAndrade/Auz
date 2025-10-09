using Infra.RawQueryResult;

namespace Application.Messaging.Response.Medico
{
    public class ObterMedicoDetalhadoResponse
    {
        public Domain.Entidades.Medico Medico { get; set; }

        public List<ListarAtendimentosPorMedicoRawQuery> Atendimentos {  get; set; }
    }
}
