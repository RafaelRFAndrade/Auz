using Application.Messaging.Exception;
using Azure.Core;

namespace Application.Messaging.Request.Atendimento
{
    public class CadastroAtendimentoRequest
    {
        public Guid CodigoPaciente { get; set; }
        public Guid CodigoMedico { get; set; }
        public string Descricao { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new AuzException("Descrição é campo obrigatório");

            if (CodigoPaciente == Guid.Empty)
                throw new AuzException("Paciente precisa ser informado");

            if (CodigoMedico == Guid.Empty)
                throw new AuzException("Medico precisa ser informado");
        }
    }
}
