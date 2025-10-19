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
        public string Nome { get; set; } = null!;

        public DateTime DtInclusao { get; set; }

        public DateTime DtSituacao { get; set; }

        public bool Admin { get; set; }

        [StringLength(150)]
        public string? RazaoSocial { get; set; }

        [StringLength(14)]
        public string? Cnpj { get; set; }

        [StringLength(9)]
        public string? Cep { get; set; }

        [StringLength(150)]
        public string? Logradouro { get; set; }

        [StringLength(20)]
        public string? Numero { get; set; }

        [StringLength(100)]
        public string? Complemento { get; set; }

        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(2)]
        public string? Uf { get; set; }

        [StringLength(12)]
        public string? Telefone { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public Guid? CodigoUsuarioResponsavel { get; set; }

        public Guid? CodigoUsuarioInclusao { get; set; }

        public Guid? CodigoUsuarioAtualizacao { get; set; }
    }
}
