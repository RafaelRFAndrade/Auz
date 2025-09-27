using Application.Messaging.Request;
using Application.Messaging.Request.Paciente;
using Application.Messaging.Response.Paciente;
using Domain.Entidades;
using Infra.RawQueryResult;

namespace Application.Interfaces
{
    public interface IPacienteService
    {
        void Cadastrar(CadastroPacienteRequest request, Guid codigoUsuario);
        ListarPacienteResponse Listar(ListarRequest request, Guid codigoUsuario);
        void Atualizar(AtualizarPacienteRequest request, Guid codigoUsuario);
        void Desativar(DesativarPacienteRequest request, Guid codigoUsuario);
        Paciente Obter(Guid codigoPaciente, Guid codigoUsuario);
        Paciente ObterPorDocumentoFederal(string documentoFederal, Guid codigoParceiro);
        List<ListarDocumentosRawQuery> BuscarDocumentos(string documentoFederal, Guid codigoParceiro);
    }
}
