using Application.Interfaces;
using Application.Messaging.Exception;
using Application.Messaging.Request.Parceiro;
using AutoMapper;
using Domain.Entidades;
using Infra.Repositories.Parceiro;

namespace Application.Services
{
    public class ParceiroService : IParceiroService
    {
        private readonly IParceiroRepository _parceiroRepository;
        private readonly IMapper _mapper;

        public ParceiroService(IParceiroRepository parceiroRepository,
            IMapper mapper)
        {
            _parceiroRepository = parceiroRepository;
            _mapper = mapper;
        }

        public Parceiro Obter(Guid codigo)
        {
            return _parceiroRepository.Obter(codigo);
        }

        public void Atualizar(AtualizarParceiroRequest request, Guid codigoParceiro)
        {
            if (request.Codigo != codigoParceiro)
                throw new AuzException("Não é possível alterar outros parceiros.");

            var parceiro = _parceiroRepository.Obter(codigoParceiro);

            _mapper.Map(request, parceiro);

            _parceiroRepository.Atualizar(parceiro);
        }
    }
}
