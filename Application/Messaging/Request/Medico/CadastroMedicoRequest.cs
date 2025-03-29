using Application.Helpers;
using Application.Messaging.Exception;

namespace Application.Messaging.Request.Medico
{
    public class CadastroMedicoRequest
    {
        public string? Nome { get; set; }
        public string? CRM { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? DocumentoFederal { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new AuzException("Nome ausente");

            if (string.IsNullOrWhiteSpace(CRM))
                throw new AuzException("CRM ausente");

            if (string.IsNullOrWhiteSpace(Email))
                throw new AuzException("Email ausente");

            if (string.IsNullOrWhiteSpace(DocumentoFederal) || !DocumentoFederal.ValidarCPF())
                throw new AuzException("Documento Federal inválido");

            if (string.IsNullOrWhiteSpace(Telefone) || !Telefone.ValidarCelular())
                throw new AuzException("Telefone ausente");
        }
    }
}
