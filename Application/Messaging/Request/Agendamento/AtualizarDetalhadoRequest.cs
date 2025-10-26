using Domain.Enums;

namespace Application.Messaging.Request.Agendamento
{
    public class AtualizarDetalhadoRequest
    {
        public Guid Codigo { get; set; }

        public DateTime DtAgendamento { get; set; }

        public Situacao Situacao { get; set; }

        public string? Descricao { get; set; }

        public string? Observacao { get; set; }

        public DateTime? DtConfirmacao { get; set; }

        public string? MotivoCancelamento { get; set; }

        public PrioridadeAgendamento? Prioridade { get; set; }
    }
}
