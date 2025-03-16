using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entidades
{
    [Table("Atendimento")]
    public class Atendimento
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public Guid Codigo { get; set; }
        [Required]
        public Guid CodigoMedico { get; set; }
        [Required]
        public Guid CodigoPaciente { get; set; }
        [Required]
        public Guid CodigoUsuario { get; set; }
        public Situacao Situacao { get; set; }
        public DateTime DtInclusao { get; set; }
        public DateTime? DtSituacao { get; set; }
        public string Descricao { get; set; }
    }
}
