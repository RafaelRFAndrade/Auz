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

        public List<UsuariosRawQuery> ObterUsuariosPorParceiro(Guid codigoParceiro, int pagina, int itens)
        {
            const string sql =
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
                ORDER BY DtInclusao DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY
                """;

            return Database.SqlQueryRaw<UsuariosRawQuery>(sql, codigoParceiro, pagina == 1 ? 0 : pagina * 25, itens).ToList();
        }

        public CountRawQuery ObterTotalizador(Guid codigoParceiro)
        {
            const string sql =
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
