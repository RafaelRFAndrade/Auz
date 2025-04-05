using Application.Helpers;
using Application.Messaging.Exception;

namespace Application.Messaging.Request.Paciente
{
    public class AtualizarPacienteRequest
    {
        public Guid CodigoPaciente { get; set; }
        public string Nome { get; set; }
        public string DocumentoFederal { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }

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
