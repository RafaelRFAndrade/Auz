namespace Infra.RawQueryResult
{
    public class DocumentoRawQuery
    {
        public Guid Codigo { get; set; } 

        public string TipoEntidade { get; set; }

        public string NomeArquivo { get; set; }

        public string? TipoConteudo { get; set; }

        public long? TamanhoBytes { get; set; }

        public DateTime DataUpload { get; set; } 
    }
}
