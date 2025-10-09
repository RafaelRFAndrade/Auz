using Application.Messaging.Request;
using Application.Messaging.Response;
using Application.Messaging.Response.Documento;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IDocumentoService
    {
        Task<ResponseBase> InserirDocumento(UploadDocumentoRequest uploadDocumentoRequest, Guid codigoUsuario, TipoEntidadeUpload tipoEntidadeUpload);
        Task<DadosDocumentoResponse> ObterDocumento(Guid codigoDocumento);
    }
}
