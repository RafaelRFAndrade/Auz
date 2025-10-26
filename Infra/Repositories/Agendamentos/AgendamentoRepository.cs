using Domain.Entidades;
using Domain.Enums;
using Infra.Helper;
using Infra.RawQueryResult;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Agendamentos
{
    public class AgendamentoRepository : RepositoryBase, IAgendamentoRepository
    {
        public AgendamentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Agendamento agendamento)
        {
            Add(agendamento);
            SaveChanges();
        }

        public List<ObterAgendamentosRawQuery> ObterAgendamentosPorParceiro(Guid codigoParceiro, int pagina, int itensPorPagina)
        {
            const string sql =
                """
                SELECT
                	age.Descricao as NomeAgendamento,
                	age.Situacao,
                	ate.Descricao as NomeAtendimento,
                	me.Nome as NomeMedico,
                	pa.Nome as NomePaciente
                FROM 	
                	dbo.Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	dbo.Atendimento AS ate WITH(NOLOCK) ON ate.Codigo = age.CodigoAtendimento
                INNER JOIN 
                	dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN 
                	dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                WHERE 
                	age.CodigoUsuario = us.Codigo
                AND DtAgendamento BETWEEN @p1 AND @p2
                ORDER BY age.DtAgendamento DESC OFFSET @p3 ROWS FETCH NEXT @p4 ROWS ONLY
                """
            ;
            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<ObterAgendamentosRawQuery>(sql,
                codigoParceiro,
                DatetimeHelper.NormalizarInicioSemana(DateTime.Now),
                DatetimeHelper.NormalizarFimSemana(DateTime.Now), offset, itensPorPagina).ToList();
        }

        public CountRawQuery ObterTotalizadorAgendamentosPorParceiro(Guid codigoParceiro)
        {
            const string sql =
                """
                SELECT
                	COUNT(age.Codigo) as 'Count'
                FROM 	
                	dbo.Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	dbo.Atendimento AS ate WITH(NOLOCK) ON ate.Codigo = age.CodigoAtendimento
                INNER JOIN 
                	dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN 
                	dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                INNER JOIN 
                    dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                WHERE 
                	age.CodigoUsuario = us.Codigo
                AND DtAgendamento BETWEEN @p1 AND @p2
                """
            ;

            return Database.SqlQueryRaw<CountRawQuery>(sql,
                codigoParceiro,
                DatetimeHelper.NormalizarInicioSemana(DateTime.Now),
                DatetimeHelper.NormalizarFimSemana(DateTime.Now)).FirstOrDefault();
        }

        public List<AgendamentoRawQueryResult> ObterAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial)
        {
            const string sql =
                """
                SELECT 
                    age.Codigo as CodigoAgendamento,
                    age.DtAgendamento,
                   	age.Descricao AS 'NomeAgendamento',
                	me.Nome AS 'NomeMedico',
                	me.CRM,
                	pa.Nome AS 'NomePaciente',
                	pa.Email AS 'EmailPaciente'
                FROM 	
                	dbo.Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                INNER JOIN 
                	dbo.Atendimento AS ate WITH(NOLOCK) ON ate.Codigo = age.CodigoAtendimento
                INNER JOIN 
                	dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN 
                	dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                WHERE 
                	age.CodigoUsuario = us.Codigo AND
                    age.Situacao = @p1            AND
                    DtAgendamento BETWEEN @p2 AND @p3
                """;

            var ultimoDiaDoMes = new DateTime(diaInicial.Year, diaInicial.Month, DateTime.DaysInMonth(diaInicial.Year, diaInicial.Month)).AddDays(1).AddTicks(-1);

            return Database.SqlQueryRaw<AgendamentoRawQueryResult>(sql, codigoParceiro, Situacao.Ativo, diaInicial, ultimoDiaDoMes).ToList();
        }

        public CountRawQuery ObterQtdAgendamentosPorParceiro(Guid codigoParceiro, DateTime diaInicial)
        {
            const string sql =
                """
                SELECT 
                    COUNT(age.Codigo) AS 'Count'
                FROM 	
                	dbo.Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	dbo.Usuario AS us WITH(NOLOCK) ON us.CodigoParceiro = @p0
                INNER JOIN 
                	dbo.Atendimento AS ate WITH(NOLOCK) ON ate.Codigo = age.CodigoAtendimento
                INNER JOIN 
                	dbo.Medico AS me WITH(NOLOCK) ON me.Codigo = ate.CodigoMedico
                INNER JOIN 
                	dbo.Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                WHERE 
                	age.CodigoUsuario = us.Codigo AND
                    age.Situacao = @p1            AND
                    DtAgendamento BETWEEN @p2 AND @p3
                """;

            var ultimoDiaDoMes = new DateTime(diaInicial.Year, diaInicial.Month, DateTime.DaysInMonth(diaInicial.Year, diaInicial.Month)).AddDays(1).AddTicks(-1);

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoParceiro, Situacao.Ativo, diaInicial, ultimoDiaDoMes).FirstOrDefault();
        }

        public bool VerificarDisponibilidade(Guid codigoAtendimento, DateTime dataAgendamento)
        {
            const string sql =
                """
                SELECT COUNT(age.Codigo) AS 'Count' FROM Agendamento AS age 
                INNER JOIN Atendimento AS ate on Ate.Codigo = @p0
                INNER JOIN Atendimento AS Ate2 on ate2.CodigoMedico = ate.CodigoMedico
                WHERE age.CodigoAtendimento = ate2.Codigo AND age.DtAgendamento BETWEEN @p1 AND @p2 
                AND age.Situacao = @p3
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoAtendimento, dataAgendamento.AddHours(-1), dataAgendamento.AddHours(1), Situacao.Ativo).FirstOrDefault().Count > 0;
        }

        public List<ObterAgendamentosPorAtendimentoRawQuery> ObterAgendamentosPorAtendimento(Guid codigoAtendimento, int pagina, int itensPorPagina)
        {
            const string sql =
                """
                SELECT 
                    Descricao, DtAgendamento, Situacao 
                FROM 
                    Agendamento 
                WHERE 
                    CodigoAtendimento = @p0     
                ORDER BY DtAgendamento DESC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY
                """;

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<ObterAgendamentosPorAtendimentoRawQuery>(sql, codigoAtendimento, offset, itensPorPagina).ToList();
        }

        public CountRawQuery ObterTotalizadorAgendamentosPorAtendimento(Guid codigoAtendimento)
        {
            const string sql =
                """
                SELECT 
                    COUNT(Codigo) as 'Count'
                FROM 
                    Agendamento 
                WHERE 
                    CodigoAtendimento = @p0     
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoAtendimento).FirstOrDefault();
        }

        public List<AgendamentoOperacionalRawQuery> ObterOperacional(Guid codigoUsuario, Guid codigoMedico, int pagina, int itensPorPagina)
        {
            const string sql =
                """
                SELECT 
                	age.Codigo as 'CodigoAgendamento',
                	pa.Nome AS 'NomePaciente',
                	ate.Descricao AS 'NomeAtendimento',
                	age.DtAgendamento AS 'DataAgendamento'
                FROM 
                	Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	Atendimento AS ate WITH(NOLOCK) ON ate.CodigoUsuario = @p0
                	AND Ate.CodigoMedico = @p1
                INNER JOIN Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                WHERE
                	age.CodigoAtendimento = ate.Codigo
                ORDER BY age.DtAgendamento DESC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY
                """;

            var offset = (pagina - 1) * itensPorPagina;

            return Database.SqlQueryRaw<AgendamentoOperacionalRawQuery>(sql, codigoUsuario, codigoMedico, offset, itensPorPagina).ToList();
        }

        public CountRawQuery ObterTotalizadorOperacional(Guid codigoUsuario, Guid codigoMedico)
        {
            const string sql =
                """
                SELECT 
                   COUNT(age.Codigo) AS 'Count'
                FROM 
                	Agendamento AS age WITH(NOLOCK)
                INNER JOIN 
                	Atendimento AS ate WITH(NOLOCK) ON ate.CodigoUsuario = @p0
                	AND Ate.CodigoMedico = @p1
                INNER JOIN Paciente AS pa WITH(NOLOCK) ON pa.Codigo = ate.CodigoPaciente
                WHERE
                	age.CodigoAtendimento = ate.Codigo
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoUsuario, codigoMedico).FirstOrDefault();
        }

        public Agendamento Obter(Guid codigoAgendamento)
        {
            const string sql =
                """
                SELECT 
                    * 
                FROM 
                    Agendamento 
                WHERE 
                    Codigo = @p0      
                """;

            return Database.SqlQueryRaw<Agendamento>(sql, codigoAgendamento).FirstOrDefault();
        }

        public void Atualizar(Agendamento agendamento)
        {
            Update(agendamento);
            SaveChanges();
        }
    }
}
