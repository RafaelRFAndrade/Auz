namespace Infra.RawQueryResult
{
    public class AgendamentoOperacionalRawQuery
    {
        public Guid CodigoAgendamento { get; set; }
        public string NomePaciente { get; set; }
        public string NomeAtendimento { get; set; }
        public DateTime DataAgendamento { get; set; }
    }
}
