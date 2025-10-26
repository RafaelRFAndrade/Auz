using Domain.Entidades;
using Domain.Enums;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Documentos
{
    public class DocumentoRepository : RepositoryBase, IDocumentoRepository
    {
        public DocumentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Documento documento)
        {
            Add(documento);
            SaveChanges();
        }

        public List<DocumentoRawQuery> ObterDocumentosPorCodigoEntidade(Guid codigoEntidade, int pagina, int itensPorPagina)
        {
            const string sql =
               """
                SELECT 
                    do.Codigo,
                    do.TipoEntidade,
                    do.NomeArquivo,
                    do.TipoConteudo,
                    do.TamanhoBytes,
                    do.DataUpload 
                FROM 
                    Documento AS do WITH(NOLOCK) 
                WHERE 
                    do.CodigoEntidade = @p0
                ORDER BY DtInclusao DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY
                """;

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<DocumentoRawQuery>(sql, codigoEntidade, offset, itensPorPagina).ToList();
        }

        public CountRawQuery ObterTotalizadorDocumentosPorCodigoEntidade(Guid codigoEntidade)
        {
            const string sql =
               """
                SELECT 
                    COUNT(do.Codigo) as 'Count'
                FROM 
                    Documento AS do WITH(NOLOCK) 
                WHERE 
                    do.CodigoEntidade = @p0
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoEntidade).FirstOrDefault();
        }

        public ObterDadosDocumentoRawQuery ObterCaminhoPorCodigo(Guid codigoDocumento)
        {
            const string sql =
               """
                SELECT 
                    do.NomeArquivo,
                    do.CaminhoS3,
                    do.TipoConteudo
                FROM 
                    Documento AS do WITH(NOLOCK) 
                WHERE 
                    do.Codigo = @p0
                """;

            return Database.SqlQueryRaw<ObterDadosDocumentoRawQuery>(sql, codigoDocumento).FirstOrDefault();
        }

        public ObterDadosDocumentoRawQuery ObterCaminhoPorEntidade(Guid codigoDocumento, TipoDocumento tipoDocumento)
        {
            const string sql =
               """
                SELECT TOP 1
                    do.NomeArquivo,
                    do.CaminhoS3,
                    do.TipoConteudo
                FROM 
                    Documento AS do WITH(NOLOCK) 
                WHERE 
                    do.CodigoEntidade = @p0 AND
                    do.TipoDocumento = @p1
                ORDER BY 
                    do.DataUpload desc
                """;

            return Database.SqlQueryRaw<ObterDadosDocumentoRawQuery>(sql, codigoDocumento, tipoDocumento).FirstOrDefault();
        }

    }
}
