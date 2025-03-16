using Application.Messaging.Request.Paciente;

namespace Application.Interfaces
{
    public interface IPacienteService
    {
        void Cadastrar(CadastroPacienteRequest request, Guid codigoUsuario);
    }
}
