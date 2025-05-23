﻿using Domain.Entidades;
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
                WHERE 
                	age.CodigoUsuario = @p0
                AND DtAgendamento BETWEEN @p1 AND @p2
                """
            ;

            return Database.SqlQueryRaw<ObterAgendamentosRawQuery>(sql,
                codigoUsuario,
                DatetimeHelper.NormalizarInicio(DateTime.Now),
                DatetimeHelper.NormalizarFim(DateTime.Now));
        }
    }
}
