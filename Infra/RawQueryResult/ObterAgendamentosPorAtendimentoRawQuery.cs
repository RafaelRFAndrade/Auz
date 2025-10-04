using Domain.Enums;

namespace Infra.RawQueryResult
{
    public class ObterAgendamentosPorAtendimentoRawQuery
    {
        public string Descricao { get; set; }
        public DateTime DtAgendamento { get; set; }
        public Situacao Situacao { get; set; }
    }
}
