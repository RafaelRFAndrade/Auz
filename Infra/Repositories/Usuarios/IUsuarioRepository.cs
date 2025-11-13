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
        List<UsuariosRawQuery> ObterUsuariosPorParceiro(Guid codigoParceiro, int pagina, int itens, string filtro);
        CountRawQuery ObterTotalizador(Guid codigoParceiro, string filtro);
        void Atualizar(Usuario usuario);
        Usuario? Obter(Guid codigo);
        CountRawQuery ObterQtdUsuariosPorParceiro(Guid codigoParceiro);
    }
}
