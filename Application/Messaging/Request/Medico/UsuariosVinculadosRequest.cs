namespace Application.Messaging.Request.Medico
{
    public class UsuariosVinculadosRequest : ListarRequest
    {
        public Guid CodigoMedico { get; set; }
    }
}
