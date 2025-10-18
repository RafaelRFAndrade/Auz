using Application.Helpers;
using Application.Messaging.Exception;
using Domain.Enums;

namespace Application.Messaging.Request.Paciente
{
    public class AtualizarPacienteDetalhadoRequest
    {
        public Guid CodigoPaciente { get; set; }

        public string Nome { get; set; }

        public string DocumentoFederal { get; set; }

        public string Telefone { get; set; }

        public string Email { get; set; }

        public DateTime? DataNascimento { get; set; }

        public decimal? Altura { get; set; }

        public decimal? Peso { get; set; }

        public string? ContatoEmergencia { get; set; }

        public string? Genero { get; set; }

        public TipoEstadoCivil? EstadoCivil { get; set; }

        public string? Cep { get; set; }

        public string? Logradouro { get; set; }

        public string? Numero { get; set; }

        public string? Complemento { get; set; }

        public string? Bairro { get; set; }

        public string? Cidade { get; set; }

        public string? Uf { get; set; }

        public bool PossuiEspecificacoes { get; set; }

        public string? DescricaoEspecificacoes { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new AuzException("Nome ausente");

            if (string.IsNullOrWhiteSpace(Telefone))
                throw new AuzException("Senha ausente");

            if (string.IsNullOrWhiteSpace(Email))
                throw new AuzException("Email ausente");

            if (string.IsNullOrWhiteSpace(DocumentoFederal) || !DocumentoFederal.ValidarCPF())
                throw new AuzException("Documento Federal inválido");
        }
    }
}
