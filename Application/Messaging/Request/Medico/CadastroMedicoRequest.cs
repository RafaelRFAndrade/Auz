using Application.Messaging.Exception;

namespace Application.Messaging.Request.Medico
{
    public class CadastroMedicoRequest
    {
        public string? Nome { get; set; }
        public string? CRM { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new AuzException("Nome ausente");

            if (string.IsNullOrWhiteSpace(CRM))
                throw new AuzException("CRM ausente");
        }
    }
}
