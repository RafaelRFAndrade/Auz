using Domain.Entidades;

namespace Infra.Repositories.Usuarios
{
    public interface IUsuarioRepository
    {
        List<Usuario> ListarUsuarios();
        void Inserir(Usuario usuario);
        Usuario ObterPorEmail(string email);
    }
}
