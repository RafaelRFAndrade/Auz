using Domain.Entidades;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Usuarios
{
    public class UsuarioRepository : RepositoryBase, IUsuarioRepository
    {
        public UsuarioRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Usuario usuario)
        {
            Add(usuario);
            SaveChanges();
        }

        public void Atualizar(Usuario usuario)
        {
            Update(usuario);
            SaveChanges();
        }

        public Usuario? Obter(Guid codigo)
        {
            const string sql =
                """
                SELECT TOP 1
                    *
                FROM 
                    dbo.Usuario WITH(NOLOCK)
                WHERE
                    Codigo = @p0
                """;

            return Database.SqlQueryRaw<Usuario>(sql, codigo).FirstOrDefault();
        }

        public List<Usuario> ListarUsuarios()
        {
            throw new NotImplementedException();
        }

        public Usuario ObterPorEmail(string email)
        {
            const string sql =
                """
                SELECT TOP 1
                    *
                FROM 
                    dbo.Usuario WITH(NOLOCK)
                WHERE
                    Email = @p0
                """;

            return Database.SqlQueryRaw<Usuario>(sql, email).FirstOrDefault();
        }

        public StringRawQuery ObterNome(Guid codigoUsuario)
        {
            const string sql =
                """
                SELECT 
                   Nome as String
                FROM 	
                	dbo.Usuario WITH(NOLOCK) 
                WHERE 
                	Codigo = @p0
                """;

            return Database.SqlQueryRaw<StringRawQuery>(sql, codigoUsuario).FirstOrDefault();
        }

        public List<UsuariosRawQuery> ObterUsuariosPorParceiro(Guid codigoParceiro, int pagina, int itens, string filtro)
        {
            string sql =
                """
                SELECT 
                    Codigo,
                    CodigoParceiro,
                    Situacao,
                    Nome,
                    Email,
                    DtInclusao,
                    DtSituacao,
                    TipoPermissao
                FROM 	
                	dbo.Usuario WITH(NOLOCK) 
                WHERE 
                	CodigoParceiro = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p3, '%') OR Email LIKE CONCAT('%', @p3, '%'))";

            sql += " ORDER BY DtInclusao DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY";

            var offset = (pagina - 1) * itens;

            return Database.SqlQueryRaw<UsuariosRawQuery>(sql, codigoParceiro, offset, itens, filtro).ToList();
        }

        public CountRawQuery ObterTotalizador(Guid codigoParceiro, string filtro)
        {
            string sql =
                """
                SELECT 
                    COUNT(*) as Count
                FROM 	
                	dbo.Usuario WITH(NOLOCK) 
                WHERE 
                	CodigoParceiro = @p0
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (Nome LIKE CONCAT('%', @p1, '%') OR Email LIKE CONCAT('%', @p1, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoParceiro, filtro).FirstOrDefault();
        }

        public CountRawQuery ObterQtdUsuariosPorParceiro(Guid codigoParceiro)
        {
            string sql =
                """
                SELECT 
                    COUNT(*) as Count
                FROM 	
                	dbo.Usuario WITH(NOLOCK) 
                WHERE 
                	CodigoParceiro = @p0
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoParceiro).FirstOrDefault();
        }
    }
}
