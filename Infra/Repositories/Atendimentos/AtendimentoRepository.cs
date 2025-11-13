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

        public IEnumerable<ObterAtendimentosRawQuery> ObterAtendimentosPorCodigoParceiro(Guid codigoParceiro)
        {
            const string sql =
                """
                SELECT TOP 10
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
                    dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                WHERE 
                	ate.CodigoUsuario =  us.Codigo
                AND 
                    Situacao = @p1 
                """;

            return Database.SqlQueryRaw<ObterAtendimentosRawQuery>(sql, codigoParceiro, Situacao.Ativo);
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

        public List<ListarAtendimentosRawQuery> ListarAtendimentos(Guid codigoParceiro, int pagina, int itensPorPagina, string filtro)
        {
            string sql =
                """
                    SELECT 
                        ate.Codigo AS 'CodigoAtendimento',
                	    pa.Nome AS 'NomePaciente',
                	    me.Nome AS 'NomeMedico',
                	    ate.DtInclusao,
                	    ate.DtSituacao,
                	    ate.Descricao
                    FROM 
                	    dbo.Atendimento AS ate WITH(NOLOCK)
                    INNER JOIN
                        dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                    INNER JOIN 
                	    dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                    INNER JOIN
                        dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                    WHERE 
                        ate.CodigoUsuario = us.Codigo AND ate.Situacao = @p1
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (pa.Nome LIKE CONCAT('%', @p4, '%') OR me.Nome LIKE CONCAT('%', @p4, '%') OR ate.Descricao LIKE CONCAT('%', @p4, '%'))";

            sql += " ORDER BY ate.DtInclusao DESC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY";

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<ListarAtendimentosRawQuery>(sql, codigoParceiro, Situacao.Ativo, offset, itensPorPagina, filtro).ToList();
         }

        public CountRawQuery TotalizarAtendimentos(Guid codigoParceiro, string filtro)
        {
            string sql =
                """
                    SELECT 
                        COUNT(*) as Count
                    FROM 
                	    dbo.Atendimento AS ate WITH(NOLOCK)
                    INNER JOIN
                        dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                    INNER JOIN 
                	    dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                    INNER JOIN
                        dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                    WHERE 
                        ate.CodigoUsuario = us.Codigo AND ate.Situacao = @p1
                """;

            if (!string.IsNullOrWhiteSpace(filtro))
                sql += " AND (pa.Nome LIKE CONCAT('%', @p2, '%') OR me.Nome LIKE CONCAT('%', @p2, '%') OR ate.Descricao LIKE CONCAT('%', @p2, '%'))";

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoParceiro, Situacao.Ativo, filtro).FirstOrDefault();
        }

        public void Deletar(Atendimento atendimento)
        {
            Remove(atendimento);
            SaveChanges();
        }

        public Atendimento Obter(Guid codigoAtendimento)
        {
            const string sql =
                """
                SELECT TOP 1
                	ate.*
                FROM 	
                	dbo.Atendimento AS ate WITH(NOLOCK) 
                WHERE 
                	ate.Codigo = @p0
                """;

            return Database.SqlQueryRaw<Atendimento>(sql, codigoAtendimento).FirstOrDefault();
        }

        public void Atualizar(Atendimento atendimento)
        {
            Update(atendimento);
            SaveChanges();
        }

        public ObterAtendimentoRawQuery ObterAtendimento(Guid codigoAtendimento)
        {
            const string sql =
                """
                SELECT 
                	ate.Codigo as 'CodigoAtendimento',
                	ate.Descricao as 'DescricaoAtendimento', 
                	ate.DtInclusao,
                	ate.Situacao,
                	me.DocumentoFederal as 'DocumentoFederalMedico',
                	me.Nome as 'NomeMedico',
                	me.Email as 'EmailMedico',
                	me.Telefone as 'TelefoneMedico',
                	pa.DocumentoFederal as 'DocumentoFederalPaciente',
                	pa.Email as 'EmailPaciente',
                	pa.Nome as 'NomePaciente',
                	pa.Telefone as 'TelefonePaciente'
                FROM Atendimento AS ate WITH(NOLOCK)
                INNER JOIN Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                WHERE ate.Codigo = @p0
                """;

            return Database.SqlQueryRaw<ObterAtendimentoRawQuery>(sql, codigoAtendimento).FirstOrDefault();
        }

        public List<ListarAtendimentosPorMedicoRawQuery> ListarAtendimentosPorMedico(Guid codigoMedico)
        {
            const string sql =
                """
                    SELECT
                        ate.Codigo AS 'CodigoAtendimento',
                	    ate.DtInclusao,
                	    ate.DtSituacao,
                	    ate.Descricao
                    FROM 
                	    dbo.Atendimento AS ate WITH(NOLOCK)
                    WHERE 
                        ate.CodigoMedico = @p0 AND ate.Situacao = @p1
                    ORDER BY ate.DtInclusao DESC
                """;

            return Database.SqlQueryRaw<ListarAtendimentosPorMedicoRawQuery>(sql, codigoMedico, Situacao.Ativo).ToList();
        }
    }
}
