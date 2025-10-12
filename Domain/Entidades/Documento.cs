using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entidades
{
    [Table("Documento")]
    public class Documento
    {
        [Key]
        public long Id { get; set; } 

        [Required]
        public Guid Codigo { get; set; } = Guid.NewGuid(); 

        [Required]
        [StringLength(100)]
        public string TipoEntidade { get; set; } 

        [Required]
        public Guid CodigoEntidade { get; set; } 

        [Required]
        [StringLength(255)]
        public string NomeArquivo { get; set; } 

        [Required]
        [StringLength(500)]
        public string CaminhoS3 { get; set; } 

        [Required]
        [StringLength(100)]
        public string Bucket { get; set; } 

        [StringLength(50)]
        public string? TipoConteudo { get; set; } 

        public long? TamanhoBytes { get; set; } 

        public Guid? UsuarioUpload { get; set; } 

        [Required]
        public DateTime DataUpload { get; set; } = DateTime.Now;

        public TipoDocumento? TipoDocumento { get; set; }
    }
}
