namespace Infra.RawQueryResult
{
    public class ListarAtendimentosRawQuery
    {
        public Guid CodigoAtendimento { get; set; }
        public string NomePaciente { get; set; }
        public string NomeMedico { get; set; }
        public DateTime DtInclusao { get; set; }
        public DateTime? DtSituacao { get; set; }
        public string Descricao { get; set; }
    }
}
