using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Documentos
{
    public interface IDocumentoRepository
    {
        void Inserir(Documento documento);
        List<DocumentoRawQuery> ObterDocumentosPorCodigoEntidade(Guid codigoEntidade);
        ObterDadosDocumentoRawQuery ObterCaminhoPorCodigo(Guid codigoDocumento);
    }
}
