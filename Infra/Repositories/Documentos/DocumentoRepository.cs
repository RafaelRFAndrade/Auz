using Domain.Entidades;
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

        public List<DocumentoRawQuery> ObterDocumentosPorCodigoEntidade(Guid codigoEntidade)
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
                """;

            return Database.SqlQueryRaw<DocumentoRawQuery>(sql, codigoEntidade).ToList();
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

    }
}
