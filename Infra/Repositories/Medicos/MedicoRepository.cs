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

        public ListarMedicoRawQuery Listar(string Filtro, Guid codigoUsuario)
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

            return Database.SqlQueryRaw<ListarMedicoRawQuery>(sql, codigoUsuario).FirstOrDefault();
        }
    }
}
