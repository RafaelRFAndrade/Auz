using Domain.Entidades;
using Infra.RawQueryResult;

namespace Infra.Repositories.Usuarios
{
    public interface IUsuarioRepository
    {
        List<Usuario> ListarUsuarios();
        void Inserir(Usuario usuario);
        Usuario ObterPorEmail(string email);
        StringRawQuery ObterNome(Guid codigoUsuario);
    }
}
