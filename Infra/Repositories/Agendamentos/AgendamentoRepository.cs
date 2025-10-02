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

        public IEnumerable<ObterAgendamentosRawQuery> ObterAgendamentosPorCodigoUsuario(Guid codigoUsuario)
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
                """
            ;

            return Database.SqlQueryRaw<ObterAgendamentosRawQuery>(sql,
                codigoUsuario,
                DatetimeHelper.NormalizarInicioSemana(DateTime.Now),
                DatetimeHelper.NormalizarFimSemana(DateTime.Now));
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
                AND Situacao = @p3
                """;

            return Database.SqlQueryRaw<CountRawQuery>(sql, codigoAtendimento, dataAgendamento.AddHours(-1), dataAgendamento.AddHours(1), Situacao.Ativo).FirstOrDefault().Count > 0;
        }
    }
}
