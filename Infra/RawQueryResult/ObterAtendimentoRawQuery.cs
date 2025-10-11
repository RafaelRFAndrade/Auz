using Domain.Enums;

namespace Infra.RawQueryResult
{
    public class ObterAtendimentoRawQuery
    {
        public Guid CodigoAtendimento { get; set; }
		public string? DescricaoAtendimento { get; set; }
		public DateTime DtInclusao { get; set; }
		public Situacao Situacao { get; set; }
		public string? DocumentoFederalMedico { get; set; }
		public string? NomeMedico { get; set; }
		public string? EmailMedico {  get; set; }
		public string? TelefoneMedico { get; set; }
		public string? DocumentoFederalPaciente { get; set; }
		public string? EmailPaciente { get; set; }
		public string? NomePaciente { get; set; }
		public string? TelefonePaciente { get; set; }
    }
}
