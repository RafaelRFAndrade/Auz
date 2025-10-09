namespace Infra.RawQueryResult
{
    public class ListarAtendimentosPorMedicoRawQuery
    {
        public Guid CodigoAtendimento { get; set; }
        public DateTime DtInclusao { get; set; }
        public DateTime? DtSituacao { get; set; }
        public string Descricao { get; set; }
    }
}
