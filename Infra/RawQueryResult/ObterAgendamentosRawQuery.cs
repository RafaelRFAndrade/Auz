using Domain.Enums;

namespace Infra.RawQueryResult
{
    public class ObterAgendamentosRawQuery
    {
        public string? NomeAgendamento { get; set; }
        public Situacao Situacao { get; set; }
        public string? NomeAtendimento { get; set; }
        public string? NomeMedico { get; set; }
        public string? NomePaciente { get; set; }
    }
}
