using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entidades
{
    [Table("Parceiro")]
    public class Parceiro
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid Codigo { get; set; }

        public Situacao Situacao { get; set; }

        [StringLength(50)]
        public string Nome { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }

        public bool Admin { get; set; }
    }
}
