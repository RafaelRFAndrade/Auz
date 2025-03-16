using Application.Messaging.Request.Atendimento;

namespace Application.Interfaces
{
    public interface IAtendimentoService
    {
        void Cadastrar(CadastroAtendimentoRequest request, Guid codigoUsuario);
    }
}
