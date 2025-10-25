namespace Application.Messaging.Request.Paciente
{
    public class ObterOperacionalRequest : ListarRequest
    {
        public Guid CodigoMedicoUsuarioOperacional { get; set; }
    }
}
