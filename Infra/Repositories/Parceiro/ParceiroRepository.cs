using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Parceiro
{
    public class ParceiroRepository : RepositoryBase, IParceiroRepository
    {
        public ParceiroRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Domain.Entidades.Parceiro parceiro)
        {
            Add(parceiro);
            SaveChanges();
        }

        public void Atualizar(Domain.Entidades.Parceiro parceiro)
        {
            Update(parceiro);
            SaveChanges();
        }

        public Domain.Entidades.Parceiro Obter(Guid codigo)
        {
            const string sql =
                """
                SELECT TOP 1
                    *
                FROM 
                    dbo.Parceiro WITH(NOLOCK)
                WHERE
                    Codigo = @p0
                """;

            return Database.SqlQueryRaw<Domain.Entidades.Parceiro>(sql, codigo).FirstOrDefault();
        }
    }
}
