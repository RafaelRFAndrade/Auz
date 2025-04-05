using Domain.Entidades;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Medicos
{
    public class MedicoRepository : RepositoryBase, IMedicoRepository
    {
        public MedicoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Medico medico)
        {
            Add(medico);
            SaveChanges();
        }

        public IEnumerable<ListarMedicoRawQuery> Listar(string filtro, Guid codigoUsuario, int pagina, int itensPorPagina)
        {
            string sql =
                """
                SELECT 
                	Codigo,
                	Situacao,
                	Nome,
                	DtInclusao,
                	DtSituacao,
                	CRM,
                	Email,
                	Telefone,
                	DocumentoFederal
                FROM 
                	dbo.Medico  WITH(NOLOCK)
                WHERE 
                    CodigoUsuario = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR CRM LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            sql += " ORDER BY DtInclusao DESC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY";

            return Database.SqlQueryRaw<ListarMedicoRawQuery>(sql, codigoUsuario, filtro, pagina == 1 ? 0 : pagina, itensPorPagina);
        }

        public CountRawQuery ObterTotalizador(string filtro, Guid codigoUsuario)
        {
            string sql =
                """
                SELECT 
                	COUNT(*) as Count
                FROM 
                	dbo.Medico WITH(NOLOCK)
                WHERE 
                    CodigoUsuario = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR CRM LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, filtro).FirstOrDefault();
        }

        public Medico Obter(Guid codigo)
        {
            const string sql =
                """
                SELECT TOP 1
                    *
                FROM 
                    dbo.Medico WITH(NOLOCK)
                WHERE
                    Codigo = @p0
                """;

            return Database.SqlQueryRaw<Medico>(sql, codigo).FirstOrDefault();
        }

        public void Atualizar(Medico medico)
        {
            Update(medico);
            SaveChanges();
        }
    }
}
