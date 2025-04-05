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

        public IEnumerable<ListarPacientesRawQuery> Listar(string filtro, Guid codigoUsuario, int pagina, int itensPorPagina)
        {
            string sql =
                """
                SELECT 
                   	Codigo,
                   	Situacao,
                   	Nome,
                   	DtInclusao,
                   	DtSituacao,
                	DocumentoFederal,
                   	Telefone,
                	Email
                FROM 
                   	dbo.Paciente WITH(NOLOCK)
                WHERE 
                    CodigoUsuario =  @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            sql += " ORDER BY DtInclusao DESC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY";

            return Database.SqlQueryRaw<ListarPacientesRawQuery>(sql, codigoUsuario, filtro, pagina == 1 ? 0 : pagina, itensPorPagina);
        }

        public CountRawQuery ObterTotalizador(string filtro, Guid codigoUsuario)
        {
            string sql =
                """
                SELECT 
                	COUNT(*) as Count
                FROM 
                	dbo.Paciente WITH(NOLOCK)
                WHERE 
                    CodigoUsuario = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%') OR Telefone LIKE CONCAT('%', @p1, '%') OR DocumentoFederal LIKE CONCAT('%', @p1, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, filtro).FirstOrDefault();
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

        public void Atualizar(Paciente paciente)
        {
            Update(paciente);
            SaveChanges();
        }
    }
}
