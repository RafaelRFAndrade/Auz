using Domain.Entidades;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Pacientes
{
    public class PacienteRepository : RepositoryBase, IPacienteRepository
    {
        public PacienteRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Paciente paciente) 
        { 
            Add(paciente);
            SaveChanges();
        }

        public IEnumerable<ListarPacientesRawQuery> Listar(string filtro, Guid codigoParceiro, int pagina, int itensPorPagina)
        {
            string sql =
                """
                SELECT 
                   	pac.Codigo,
                   	pac.Situacao,
                   	pac.Nome,
                   	pac.DtInclusao,
                   	pac.DtSituacao,
                	pac.DocumentoFederal,
                   	pac.Telefone,
                	pac.Email
                FROM 
                   	dbo.Paciente as pac WITH(NOLOCK)
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                WHERE 
                    pac.CodigoUsuario = us.Codigo
                AND
                    pac.Situacao = 0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            sql += " ORDER BY DtInclusao DESC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY";

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<ListarPacientesRawQuery>(sql, codigoParceiro, filtro, offset, itensPorPagina);
        }

        public CountRawQuery ObterTotalizador(string filtro, Guid codigoParceiro)
        {
            string sql =
                """
                SELECT 
                	COUNT(pac.Codigo) as Count
                FROM 
                	dbo.Paciente AS pac WITH(NOLOCK)
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                WHERE 
                    pac.CodigoUsuario = us.Codigo
                AND
                    pac.Situacao = 0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoParceiro, filtro).FirstOrDefault();
        }

        public Paciente Obter(Guid codigo)
        {
            const string sql =
                """
                SELECT TOP 1
                    *
                FROM 
                    dbo.Paciente WITH(NOLOCK)
                WHERE
                    Codigo = @p0
                """;

            return Database.SqlQueryRaw<Paciente>(sql, codigo).FirstOrDefault();
        }

        public Paciente ObterPorDocumentoFederal(string documentoFederal, Guid codigoParceiro)
        {
            const string sql =
                """
                SELECT TOP 1
                   pa.*
                FROM 
                    dbo.Paciente AS pa WITH(NOLOCK)
                INNER JOIN 
                    Usuario AS us WITH(NOLOCK) 
                    ON us.CodigoParceiro = @p1
                WHERE
                    pa.DocumentoFederal = @p0
                AND
                    pa.Situacao = 0
                AND pa.CodigoUsuario = us.Codigo
                """;

            return Database.SqlQueryRaw<Paciente>(sql, documentoFederal, codigoParceiro).FirstOrDefault();
        }

        public void Atualizar(Paciente paciente)
        {
            Update(paciente);
            SaveChanges();
        }

        public List<ListarDocumentosRawQuery> ObterDocumentos(string DocumentoFederal, Guid codigoParceiro)
        {
            const string sql =
                """
                SELECT TOP 10
                    pa.DocumentoFederal,
                    pa.Nome,
                    pa.Email
                FROM 
                    dbo.Paciente AS pa WITH(NOLOCK)
                INNER JOIN 
                    Usuario AS us WITH(NOLOCK) 
                    ON us.CodigoParceiro = @p1
                WHERE
                    pa.DocumentoFederal LIKE CONCAT('%', @p0, '%')
                AND
                    pa.Situacao = 0
                AND pa.CodigoUsuario = us.Codigo
                """;

            return Database.SqlQueryRaw<ListarDocumentosRawQuery>(sql, DocumentoFederal, codigoParceiro).ToList();
        }

        public IEnumerable<ListarPacientesRawQuery> ObterOperacional(string filtro, Guid codigoUsuario, Guid codigoMedico, int pagina, int itensPorPagina)
        {
            string sql =
                """
                SELECT DISTINCT
                   	pac.Codigo,
                   	pac.Situacao,
                   	pac.Nome,
                   	pac.DtInclusao,
                   	pac.DtSituacao,
                	pac.DocumentoFederal,
                   	pac.Telefone,
                	pac.Email
                FROM 
                   	dbo.Paciente as pac WITH(NOLOCK)
                INNER JOIN 
                    dbo.Atendimento AS ate WITH(NOLOCK) ON ate.CodigoUsuario = @p0
                    AND ate.CodigoMedico = @p1
                WHERE 
                    pac.Codigo = ate.CodigoPaciente
                AND
                    pac.Situacao = 0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p2, '%') OR Email LIKE CONCAT('%', @p2, '%') OR Telefone LIKE CONCAT('%', @p2, '%') OR DocumentoFederal LIKE CONCAT('%', @p2, '%'))";

            sql += " ORDER BY DtInclusao DESC OFFSET @p3 ROWS FETCH NEXT @p4 ROWS ONLY";

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<ListarPacientesRawQuery>(sql, codigoUsuario, codigoMedico, filtro, offset, itensPorPagina);
        }

        public CountRawQuery ObterTotalizadorOperacional(string filtro, Guid codigoUsuario, Guid codigoMedico)
        {
            string sql =
                """
                SELECT 
                	COUNT(DISTINCT pac.Codigo) as Count
                FROM 
                	dbo.Paciente AS pac WITH(NOLOCK)
                INNER JOIN 
                    dbo.Atendimento AS ate WITH(NOLOCK) ON ate.CodigoUsuario = @p0
                    AND ate.CodigoMedico = @p1
                WHERE 
                    pac.Codigo = ate.CodigoPaciente
                AND
                    pac.Situacao = 0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p2, '%') OR Email LIKE CONCAT('%', @p2, '%') OR Telefone LIKE CONCAT('%', @p2, '%') OR DocumentoFederal LIKE CONCAT('%', @p2, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, codigoMedico, filtro).FirstOrDefault();
        }
    }
}
