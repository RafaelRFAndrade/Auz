using Infra.RawQueryResult;

namespace Application.Messaging.Response.Documento
{
    public class DadosDocumentoResponse
    {
        public ObterDadosDocumentoRawQuery DadosDocumento { get; set; }
        public byte[] Documento { get; set; }   
    }
}
