using Application.Messaging.Exception;
using Application.Messaging.Request.Paciente;
using Azure.Core;

namespace Application.Messaging.Request.Atendimento
{
    public class CadastroAtendimentoRequest
    {
        public string Descricao { get; set; }
        public string DocumentoFederalPaciente { get; set; }
        public string DocumentoFederalMedico { get; set; }
        public bool CadastrarPaciente { get; set; } = false;
        public CadastroPacienteAtendimentoRequest? NovoPaciente {  set; get; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new AuzException("Descrição é campo obrigatório");

            if (string.IsNullOrWhiteSpace(DocumentoFederalPaciente))
                throw new AuzException("Paciente precisa ser informado");

            if (string.IsNullOrWhiteSpace(DocumentoFederalMedico))
                throw new AuzException("Medico precisa ser informado");
        }
    }
}
