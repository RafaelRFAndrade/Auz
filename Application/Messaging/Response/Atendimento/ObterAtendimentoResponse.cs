using Domain.Enums;
using Infra.RawQueryResult;

namespace Application.Messaging.Response.Atendimento
{
    public class ObterAtendimentoResponse
    {
        public Guid CodigoAtendimento { get; set; }
        public string DescricaoAtendimento { get; set; }
        public DateTime DtInclusao { get; set; }
        public Situacao Situacao { get; set; }
        public string DocumentoFederalMedico { get; set; }
        public string NomeMedico { get; set; }
        public string EmailMedico { get; set; }
        public string TelefoneMedico { get; set; }
        public string DocumentoFederalPaciente { get; set; }
        public string EmailPaciente { get; set; }
        public string NomePaciente { get; set; }
        public string TelefonePaciente { get; set; }

        public List<ObterAgendamentosPorAtendimentoRawQuery> Agendamentos {  get; set; }
    }
}
