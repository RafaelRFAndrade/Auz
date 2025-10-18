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

        public Guid? CodigoUsuario { get; set; }

        public Situacao Situacao { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = null!;

        public DateTime? DtInclusao { get; set; }

        public DateTime? DtSituacao { get; set; }

        [StringLength(11)]
        public string? DocumentoFederal { get; set; }

        [StringLength(12)]
        public string? Telefone { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public DateTime? DataNascimento { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Altura { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Peso { get; set; }

        [StringLength(12)]
        public string? ContatoEmergencia { get; set; }

        [StringLength(1)]
        public string? Genero { get; set; }

        public TipoEstadoCivil? EstadoCivil { get; set; }

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

        public bool PossuiEspecificacoes { get; set; }

        [StringLength(500)]
        public string? DescricaoEspecificacoes { get; set; }
    }
}
