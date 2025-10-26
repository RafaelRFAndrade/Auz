using Infra.RawQueryResult;

namespace Application.Messaging.Response.Documento
{
    public class ListarDocumentosResponse
    {
        public List<DocumentoRawQuery> Documentos {  get; set; }
        public int TotalPaginas { get; set; }
        public int Itens { get; set; }
    }
}
