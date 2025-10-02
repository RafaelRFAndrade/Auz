using Infra.RawQueryResult;

namespace Application.Messaging.Response.Agendamento
{
    public class AgendamentosResponse : ResponseBase
    {
        public List<AgendamentoRawQueryResult> Agendamentos {  get; set; }
        public int QtdAgendamentos { get; set; }
    }
}
