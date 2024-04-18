using MotoHub.Models;

namespace MotoHub.Repositories
{
    public interface IMotorcycleRepository
    {
        IEnumerable<Motorcycle> GetAll();
        Motorcycle? GetById(int id);
        void Add(Motorcycle motorcycle);
        void Update(Motorcycle motorcycle);
        void Delete(int id);
        bool LicensePlateExists(string licensePlate);
        Motorcycle? GetByLicensePlate(string licensePlate);
    }
}
