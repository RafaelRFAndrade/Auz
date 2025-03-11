using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class UsuarioRepository : RepositoryBase, IUsuarioRepository
    {
        public UsuarioRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public List<Usuario> ListarUsuarios()
        {
            throw new NotImplementedException();
        }

        public void Inserir(Usuario usuario) => Add(usuario);
    }
}
