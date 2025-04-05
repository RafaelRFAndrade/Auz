using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Messaging.Response.Paciente;

namespace Application.Interfaces
{
    public interface IPacienteService
    {
        void Cadastrar(CadastroPacienteRequest request, Guid codigoUsuario);
        ListarPacienteResponse Listar(ListarRequest request, Guid codigoUsuario);
        void Atualizar(AtualizarPacienteRequest request, Guid codigoUsuario);
        void Desativar(DesativarPacienteRequest request, Guid codigoUsuario);
    }
}
