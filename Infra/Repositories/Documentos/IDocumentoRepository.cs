using Domain.Entidades;
using Domain.Enums;
using Infra.RawQueryResult;

namespace Infra.Repositories.Documentos
{
    public interface IDocumentoRepository
    {
        void Inserir(Documento documento);
        List<DocumentoRawQuery> ObterDocumentosPorCodigoEntidade(Guid codigoEntidade, int pagina, int itensPorPagina);
        ObterDadosDocumentoRawQuery ObterCaminhoPorCodigo(Guid codigoDocumento);
        ObterDadosDocumentoRawQuery ObterCaminhoPorEntidade(Guid codigoDocumento, TipoDocumento tipoDocumento);
        CountRawQuery ObterTotalizadorDocumentosPorCodigoEntidade(Guid codigoEntidade);
    }
}
