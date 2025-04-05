using Domain.Entidades;
using Domain.Enums;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Atendimentos
{
    public class AtendimentoRepository : RepositoryBase, IAtendimentoRepository
    {
        public AtendimentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Atendimento atendimento)
        {
            Add(atendimento);
            SaveChanges();  
        }

        public IEnumerable<ObterAtendimentosRawQuery> ObterAtendimentosPorCodigoUsuario(Guid codigoUsuario)
        {
            const string sql =
                """
                SELECT 
                	ate.Descricao AS NomeAtendimento,
                	me.Nome AS NomeMedico,
                	pa.Nome AS NomePaciente,
                	ate.DtInclusao,
                    us.Nome AS NomeUsuario
                FROM 	
                	dbo.Atendimento AS ate WITH(NOLOCK) 
                INNER JOIN 
                	dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN 
                	dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.Codigo = ate.CodigoUsuario
                WHERE 
                	ate.CodigoUsuario = @p0
                """;

            return Database.SqlQueryRaw<ObterAtendimentosRawQuery>(sql, codigoUsuario);
        }

        public bool ValidarAtendimentoAtivosPorMedico(Guid codigoMedico)
        {
            const string sql =
                """
                    SELECT 
                	    COUNT(*) AS Count
                    FROM 
                	    dbo.Atendimento
                    WHERE 
                        CodigoMedico = @p0 AND 
                        Situacao = @p1 
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoMedico, Situacao.Ativo).FirstOrDefault()?.Count > 0;
        }

        public bool ValidarAtendimentoAtivosPorPaciente(Guid codigoPaciente)
        {
            const string sql =
                """
                    SELECT 
                	    COUNT(*) AS Count
                    FROM 
                	    dbo.Atendimento
                    WHERE 
                        CodigoPaciente = @p0 AND 
                        Situacao = @p1 
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoPaciente, Situacao.Ativo).FirstOrDefault()?.Count > 0;
        }
    }
}
