namespace Application.Messaging.Request.Documento
{
    public class ListarDocumentosRequest : ListarRequest    
    {
        public Guid CodigoEntidade { get; set; }
    }
}
