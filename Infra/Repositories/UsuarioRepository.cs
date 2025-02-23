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
            return Usuario.ToList();
        }
    }
}
