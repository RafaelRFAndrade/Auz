using Domain.Entidades;

namespace Infra.Repositories.Atendimentos
{
    public interface IAtendimentoRepository
    {
        void Inserir(Atendimento atendimento);
    }
}
