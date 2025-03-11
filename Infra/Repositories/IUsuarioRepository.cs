using Domain.Entidades;

namespace Infra.Repositories
{
    public interface IUsuarioRepository
    {
        List<Usuario> ListarUsuarios();
        void Inserir(Usuario usuario);
    }
}
