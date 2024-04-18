using MotoHub.DTOs;

namespace MotoHub.Services
{
    public interface IMotorcycleService
    {
        IEnumerable<MotorcycleDTO> GetAllMotorcycles();
        MotorcycleDTO GetMotorcycleByLicensePlate(string licensePlate);
        void CreateMotorcycle(MotorcycleDTO motorcycleDto);
        void UpdateMotorcycle(string licensePlate, MotorcycleDTO motorcycleDto);
        void DeleteMotorcycle(string licensePlate);
        bool LicensePlateExists(string licensePlate);
    }
}
