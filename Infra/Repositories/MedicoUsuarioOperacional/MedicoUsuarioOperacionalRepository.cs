using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.MedicoUsuarioOperacional
{
    public class MedicoUsuarioOperacionalRepository : RepositoryBase, IMedicoUsuarioOperacionalRepository
    {
        public MedicoUsuarioOperacionalRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Domain.Entidades.MedicoUsuarioOperacional medicoUsuarioOperacional)
        {
            Add(medicoUsuarioOperacional);
            SaveChanges();
        }

        public void Atualizar(Domain.Entidades.MedicoUsuarioOperacional medicoUsuarioOperacional)
        {
            Update(medicoUsuarioOperacional);
            SaveChanges();
        }

        public List<ObterMedicoUsuarioRawQuery> Listar(Guid codigoUsuario, int pagina, int itens, string filtro)
        {
            string sql =
                """
                SELECT 
                    muo.Codigo,
                    me.Nome as 'NomeMedico',
                    muo.DtInclusao as 'DataCriacao',
                    muo.DtSituacao as 'UltimaAtualizacao',
                    muo.Ativo
                FROM 
                    dbo.MedicoUsuarioOperacional AS muo WITH(NOLOCK)
                INNER JOIN 
                    dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = muo.CodigoMedico
                WHERE
                    muo.CodigoUsuario = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (me.Nome LIKE CONCAT('%', @p3, '%')";

            sql += " ORDER BY muo.DtInclusao DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY";

            var offset = (pagina - 1) * itens;

            return Database.SqlQueryRaw<ObterMedicoUsuarioRawQuery>(sql, codigoUsuario, offset, itens, filtro).ToList();
        }

        public CountRawQuery TotalizarRelacionamentos(Guid codigoUsuario, string filtro)
        {
            string sql =
                """
                SELECT 
                    COUNT(*) as Count
                FROM 
                    dbo.MedicoUsuarioOperacional WITH(NOLOCK)
                WHERE
                    CodigoUsuario = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%')";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, filtro).FirstOrDefault();
        }

        public bool VerificarRelacionamento(Guid codigoUsuario, Guid codigoMedico)
        {
            const string sql =
                """
                SELECT 
                    COUNT(*) as Count
                FROM 
                    dbo.MedicoUsuarioOperacional WITH(NOLOCK)
                WHERE
                    CodigoUsuario = @p0 AND CodigoMedico = @p1
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, codigoMedico).FirstOrDefault().Count > 0;
        }

        public Domain.Entidades.MedicoUsuarioOperacional Obter(Guid codigo)
        {
            const string sql =
                """
                SELECT 
                    *
                FROM 
                    dbo.MedicoUsuarioOperacional WITH(NOLOCK)
                WHERE
                    Codigo = @p0
                """;

            return Database.SqlQueryRaw<Domain.Entidades.MedicoUsuarioOperacional>(sql, codigo).FirstOrDefault();
        }

        public List<UsuariosVinculadosRawQuery> ListarUsuariosVinculados(Guid codigoMedico, int pagina, int itens)
        {
            string sql =
                """
                SELECT 
                    us.Nome as 'NomeUsuario',
                    us.Email as 'EmailUsuario',
                    muo.DtInclusao as 'DtVinculo'
                FROM 
                    dbo.MedicoUsuarioOperacional AS muo WITH(NOLOCK)
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.Codigo = muo.CodigoUsuario
                WHERE
                    muo.CodigoMedico = @p0 AND muo.Ativo = 1
                """;

            sql += " ORDER BY muo.DtInclusao DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY";

            var offset = (pagina - 1) * itens;

            return Database.SqlQueryRaw<UsuariosVinculadosRawQuery>(sql, codigoMedico, offset, itens).ToList();
        }

        public CountRawQuery TotalizarUsuariosVinculados(Guid codigoMedico)
        {
            string sql =
                """
                SELECT 
                    COUNT(muo.Codigo) as 'Count'
                FROM 
                    dbo.MedicoUsuarioOperacional AS muo WITH(NOLOCK)
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.Codigo = muo.CodigoUsuario
                WHERE
                    muo.CodigoMedico = @p0 AND muo.Ativo = 1
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoMedico).FirstOrDefault();
        }
    }
}
