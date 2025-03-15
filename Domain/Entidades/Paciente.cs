using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entidades
{
    [Table("Paciente")]
    public class Paciente
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid Codigo { get; set; }

        public Guid CodigoMedico { get; set; }

        public Guid CodigoUsuario { get; set; }

        public Situacao Situacao { get; set; }

        [StringLength(50)]
        public string Nome { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }
    }
}
