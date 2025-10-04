namespace Application.Messaging.Request
{
    public class UploadDocumentoRequest
    {
        /// <summary>
        /// Nome do arquivo que será salvo no S3, ex: "contrato.pdf"
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo do arquivo em Base64
        /// </summary>
        public string Base64Content { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME do arquivo, ex: "application/pdf", "image/png"
        /// Opcional, padrão "application/octet-stream"
        /// </summary>
        public string? ContentType { get; set; }

        public Guid CodigoEntidade { get; set; }
    }
}
