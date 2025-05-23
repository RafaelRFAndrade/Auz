﻿using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Agendamentos
{
    public interface IAgendamentoRepository
    {
        IEnumerable<ObterAgendamentosRawQuery> ObterAgendamentosPorCodigoUsuario(Guid codigoUsuario);
        void Inserir(Agendamento agendamento);
    }
}
