using Application.Messaging.Request;
using Application.Messaging.Request.Usuario;
using Application.Messaging.Response.Usuario;
using Domain.Entidades;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        void Cadastrar(CadastroUsuarioRequest request);
        Usuario Login(LoginRequest request);
        ValoresHomeResponse TrazerHome(Guid codigoUsuario, Guid codigoParceiro);
        ObterUsuariosParceiroResponse ObterUsuariosPorParceiro(Guid codigoParceiro, ObterUsuariosPorParceiroRequest request);
        void CadastrarParceiroJaExistente(CadastroUsuarioParceiroJaExistenteRequest request, Guid codigoParceiro);
        void Desativar(DesativarUsuarioRequest request);
        void Atualizar(AtualizarUsuarioRequest request);
        void RelacionarUsuarioMedico(RelacionarMedicoUsuarioRequest request, TipoPermissao tipoPermissao, Guid codigoUsuario);
        ObterMedicoUsuarioResponse ObterRelacionamentos(ListarRequest listarRequest, Guid codigoUsuario);
    }
}
