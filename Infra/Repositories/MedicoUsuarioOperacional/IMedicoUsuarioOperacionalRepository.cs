using Infra.RawQueryResult;

namespace Infra.Repositories.MedicoUsuarioOperacional
{
    public interface IMedicoUsuarioOperacionalRepository
    {
        void Inserir(Domain.Entidades.MedicoUsuarioOperacional medicoUsuarioOperacional);
        void Atualizar(Domain.Entidades.MedicoUsuarioOperacional medicoUsuarioOperacional);
        List<ObterMedicoUsuarioRawQuery> Listar(Guid codigoUsuario, int pagina, int itens, string filtro);
        CountRawQuery TotalizarRelacionamentos(Guid codigoUsuario, string filtro);
    }
}
