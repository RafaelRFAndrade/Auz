using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entidades
{
    [Table("MedicoUsuarioOperacional")]
    public class MedicoUsuarioOperacional
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid Codigo { get; set; }

        [Required]
        public Guid CodigoMedico { get; set; }

        [Required]
        public Guid CodigoUsuario { get; set; }

        public TipoVisualizacao Permissao { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime? DtSituacao { get; set; }

        public bool Ativo { get; set; }
    }
}
