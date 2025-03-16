using Domain.Entidades;
using Infra.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Pacientes
{
    public class PacienteRepository : RepositoryBase, IPacienteRepository
    {
        public PacienteRepository(DbContextOptions<RepositoryBase> options) : base(options)
        {
        }

        public void Inserir(Paciente paciente) 
        { 
            Add(paciente);
            SaveChanges();
        }

    }
}
