using Application.Messaging.Request;
using Application.Messaging.Request.Medico;
using Application.Messaging.Response.Medico;
using Domain.Entidades;
using Infra.RawQueryResult;

namespace Application.Interfaces
{
    public interface IMedicoService
    {
        void Cadastrar(CadastroMedicoRequest request, Guid codigoUsuario);
        ListarMedicosResponse Listar(ListarRequest request, Guid codigoUsuario);
        void Atualizar(AtualizarMedicoRequest request, Guid codigoUsuario);
        void Desativar(DesativarMedicoRequest request, Guid codigoUsuario);
        Medico Obter(Guid codigoMedico, Guid codigoUsuario);
        Medico ObterPorDocumento(string documentoFederal, Guid codigoParceiro);
        List<ListarDocumentosRawQuery> BuscarDocumentos(string documentoFederal, Guid codigoParceiro);
        ObterMedicoDetalhadoResponse ObterMedicoDetalhado(Guid codigoMedico);
        void AtualizarCompleto(AtualizarCompletoRequest request, Guid codigoUsuario);
    }
}
