using Application.Messaging.Request.Atendimento;
using Application.Messaging.Response.Atendimento;

namespace Application.Interfaces
{
    public interface IAtendimentoService
    {
        void Cadastrar(CadastroAtendimentoRequest request, Guid codigoUsuario, Guid codigoParceiro);
        ListarAtendimentosResponse ListarAtendimentos(ListarAtendimentosRequest request, Guid codigoUsuario, Guid codigoParceiro);
    }
}
