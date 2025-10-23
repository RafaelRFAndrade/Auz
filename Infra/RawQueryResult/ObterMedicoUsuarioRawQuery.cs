namespace Infra.RawQueryResult
{
    public class ObterMedicoUsuarioRawQuery
    {
        public Guid Codigo { get; set; }
        public string NomeMedico { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimaAtualizacao { get; set; }
        public bool Ativo {  get; set; }
    }
}
