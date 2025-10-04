using Domain.Entidades;

namespace Infra.Repositories.Documentos
{
    public interface IDocumentoRepository
    {
        void Inserir(Documento documento);
    }
}
