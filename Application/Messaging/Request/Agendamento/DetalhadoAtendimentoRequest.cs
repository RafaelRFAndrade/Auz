namespace Application.Messaging.Request.Agendamento
{
    public class DetalhadoAtendimentoRequest : ListarRequest
    {
        public Guid CodigoAtendimento { get; set; }
    }
}
