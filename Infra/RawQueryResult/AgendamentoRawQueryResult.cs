namespace Infra.RawQueryResult
{
    public class AgendamentoRawQueryResult
    {
        public Guid CodigoAgendamento { get; set; }
        public DateTime DtAgendamento { get; set; }
        public string NomeAgendamento { get; set; }
        public string NomeMedico { get; set; }
        public string CRM { get; set; }
        public string NomePaciente { get; set; }
        public string EmailPaciente { get; set; }
    }
}
