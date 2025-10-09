using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entidades
{
    [Table("Medico")]
    public class Medico
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid Codigo { get; set; }

        public Guid CodigoUsuario { get; set; }

        public Situacao Situacao { get; set; }

        [StringLength(50)]
        public string? Nome { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime? DtSituacao { get; set; }

        [StringLength(15)]
        public string CRM { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(12)]
        public string? Telefone { get; set; }

        [StringLength(11)]
        public string? DocumentoFederal { get; set; }

        [StringLength(50)]
        public string? Especialidade { get; set; }

        public int? DiasAtendimento { get; set; }

        [StringLength(50)]
        public string? TipoContrato { get; set; }

        [Column(TypeName = "decimal(13,2)")]
        public decimal? ValorConsulta { get; set; }
    }
}
