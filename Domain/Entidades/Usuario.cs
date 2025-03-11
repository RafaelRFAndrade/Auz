using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entidades
{
    [Table("dbo.Usuario")]
    public class Usuario
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid Codigo { get; set; }

        public Situacao Situacao { get; set; }

        [StringLength(50)]
        public string Nome { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Senha { get; set; }

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }

        public TipoPermissao TipoPermissao {  get; set; }
    }
}
