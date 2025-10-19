namespace Infra.Repositories.Parceiro
{
    public interface IParceiroRepository
    {
        void Inserir(Domain.Entidades.Parceiro parceiro);
        void Atualizar(Domain.Entidades.Parceiro parceiro);
        Domain.Entidades.Parceiro Obter(Guid codigo);
    }
}
