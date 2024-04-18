using AutoMapper;
using MotoHub.DTOs;
using MotoHub.Models;
using MotoHub.Repositories;

namespace MotoHub.Services
{
    public class MotorcycleService : IMotorcycleService
    {
        private readonly IMotorcycleRepository _repository;
        private readonly IMapper _mapper;

        public MotorcycleService(IMotorcycleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IEnumerable<MotorcycleDTO> GetAllMotorcycles()
        {
            var motorcycles = _repository.GetAll();
            return _mapper.Map<IEnumerable<MotorcycleDTO>>(motorcycles);
        }

        public MotorcycleDTO GetMotorcycleByLicensePlate(string licensePlate)
        {
            var motorcycle = _repository.GetByLicensePlate(licensePlate);
            return _mapper.Map<MotorcycleDTO>(motorcycle);
        }

        public void CreateMotorcycle(MotorcycleDTO motorcycleDto)
        {
            var motorcycle = _mapper.Map<Motorcycle>(motorcycleDto);
            _repository.Add(motorcycle);
        }

        public void UpdateMotorcycle(string licensePlate, MotorcycleDTO motorcycleDto)
        {
            var existingMotorcycle = _repository.GetByLicensePlate(licensePlate);
            if (existingMotorcycle == null)
            {
                // Lidar com a situação em que a motocicleta não existe
                return;
            }

            existingMotorcycle.Year = motorcycleDto.Year;
            existingMotorcycle.Model = motorcycleDto.Model;
            existingMotorcycle.LicensePlate = motorcycleDto.LicensePlate;

            _repository.Update(existingMotorcycle);
        }

        public void DeleteMotorcycle(string licensePlate)
        {
            var existingMotorcycle = _repository.GetByLicensePlate(licensePlate);
            if (existingMotorcycle == null)
            {
                // Lidar com a situação em que a motocicleta não existe
                return;
            }

            // Lidar com a situação em que a motocicleta tem locações ativas no microserviço de locação
            // Remova o comentário quando o microserviço de locação estiver pronto para lidar com isso
            /*if (_repository.HasActiveRentals(existingMotorcycle.Id))
            {
                // Lidar com a situação em que a motocicleta tem locações ativas
                return;
            }*/

            _repository.Delete(existingMotorcycle.Id);
        }


        public bool LicensePlateExists(string licensePlate)
        {
            return _repository.LicensePlateExists(licensePlate);
        }
    }
}
