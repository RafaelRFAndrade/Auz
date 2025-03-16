using Domain.Entidades;

namespace Infra.Repositories.Pacientes
{
    public interface IPacienteRepository
    {
        void Inserir(Paciente paciente);
    }
}
