using Application.Messaging.Request.Usuario;
using Application.Messaging.Response.Usuario;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        void Cadastrar(CadastroUsuarioRequest request);
        Usuario Login(LoginRequest request);
        ValoresHomeResponse CarregarRelacionamentos(Guid codigoUsuario, Guid codigoParceiro);
        ObterUsuariosParceiroResponse ObterUsuariosPorParceiro(Guid codigoParceiro, ObterUsuariosPorParceiroRequest request);
        void CadastrarParceiroJaExistente(CadastroUsuarioParceiroJaExistenteRequest request, Guid codigoParceiro);
        void Desativar(DesativarUsuarioRequest request);
        void Atualizar(AtualizarUsuarioRequest request);
    }
}
