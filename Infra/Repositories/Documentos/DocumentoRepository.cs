using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Documentos
{
    public class DocumentoRepository : RepositoryBase, IDocumentoRepository
    {
        public DocumentoRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Documento documento)
        {
            Add(documento);
            SaveChanges();
        }
    }
}
