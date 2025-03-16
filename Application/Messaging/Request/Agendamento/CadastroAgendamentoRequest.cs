using Application.Messaging.Exception;

namespace Application.Messaging.Request.Agendamento
{
    public class CadastroAgendamentoRequest
    {
        public Guid CodigoAtendimento { get; set; }
        public string Descricao { get; set; }
        public DateTime DtAgendamento { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new AuzException("Descrição é campo obrigatório");

            if (CodigoAtendimento == Guid.Empty)
                throw new AuzException("Atendamento precisa ser informado");

            if (DtAgendamento == null || DtAgendamento == DateTime.MinValue)
                throw new AuzException("Atendamento precisa ser informado");
        }
    }
}
