using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IAutenticacaoService
    {
        string Encriptador(Usuario usuario, string senha);
        string GenerateToken(Usuario usuario);
    }
}
