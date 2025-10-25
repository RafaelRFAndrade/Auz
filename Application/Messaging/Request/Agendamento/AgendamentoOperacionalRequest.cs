namespace Application.Messaging.Request.Agendamento
{
    public class AgendamentoOperacionalRequest
    {
        public Guid CodigoMedicoUsuarioOperacional {  get; set; }
        public int? Pagina { get; set; } = 1;
        public int? ItensPorPagina { get; set; } = 10;
    }
}
