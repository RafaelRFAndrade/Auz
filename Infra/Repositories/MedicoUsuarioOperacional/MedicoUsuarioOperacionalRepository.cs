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

        public List<ObterMedicoUsuarioRawQuery> Listar(Guid codigoUsuario)
        {
            const string sql =
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
                    muo.Codigo = @p0
                """;

            return Database.SqlQueryRaw<ObterMedicoUsuarioRawQuery>(sql, codigoUsuario).ToList();
        }
    }
}
