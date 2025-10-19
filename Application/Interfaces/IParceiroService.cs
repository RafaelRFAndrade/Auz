using Application.Messaging.Request.Parceiro;
using Domain.Entidades;

namespace Application.Interfaces
{
    public interface IParceiroService
    {
        Parceiro Obter(Guid codigo);
        void Atualizar(AtualizarParceiroRequest request, Guid codigoParceiro);
    }
}
