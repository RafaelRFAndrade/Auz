using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entidades
{
    [Table("Agendamento")]
    public class Agendamento
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public Guid Codigo { get; set; }
        [Required]
        public Guid CodigoAtendimento { get; set; }
        public DateTime DtAgendamento { get; set; }
        public DateTime DtInclusao { get; set; }
        public DateTime? DtSituacao { get; set; }
        public Situacao Situacao { get; set; }
        public string? Descricao { get; set; }
        public Guid CodigoUsuario { get; set; }
    }
}
