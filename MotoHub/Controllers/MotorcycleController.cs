using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoHub.DTOs;
using MotoHub.Filters;
using MotoHub.Services;

namespace MotoHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotorcyclesController : ControllerBase
    {
        private readonly IMotorcycleService _motorcycleService;


        private readonly ILogger<MotorcyclesController> _logger;

        public MotorcyclesController(IMotorcycleService motorcycleService, ILogger<MotorcyclesController> logger)
        {
            _motorcycleService = motorcycleService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Fetching all motorcycles.");
            var motorcycles = _motorcycleService.GetAllMotorcycles();
            return Ok(motorcycles);
        }

        [Authorize]
        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpGet("{licensePlate}")]
        public IActionResult GetByLicensePlate(string licensePlate)
        {
            _logger.LogInformation("Fetching motorcycle by license plate: {LicensePlate}", licensePlate);
            var motorcycle = _motorcycleService.GetMotorcycleByLicensePlate(licensePlate);
            if (motorcycle == null)
            {
                _logger.LogWarning("Motorcycle with license plate {LicensePlate} not found.", licensePlate);
                return NotFound();
            }
            return Ok(motorcycle);
        }

        [Authorize]
        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public IActionResult Create(MotorcycleDTO motorcycle)
        {
            _logger.LogInformation("Creating motorcycle with license plate {LicensePlate}.", motorcycle.LicensePlate);
            if (_motorcycleService.LicensePlateExists(motorcycle.LicensePlate))
            {
                _logger.LogWarning("License plate {LicensePlate} already exists.", motorcycle.LicensePlate);
                return Conflict("License plate already exists.");
            }

            _motorcycleService.CreateMotorcycle(motorcycle);
            return CreatedAtAction(nameof(GetByLicensePlate), new { licensePlate = motorcycle.LicensePlate }, motorcycle);
        }

        [Authorize]
        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{licensePlate}")]
        public IActionResult Update(string licensePlate, MotorcycleDTO motorcycle)
        {
            _logger.LogInformation("Updating motorcycle with license plate {LicensePlate}.", licensePlate);
            var existingMotorcycle = _motorcycleService.GetMotorcycleByLicensePlate(licensePlate);
            if (existingMotorcycle == null)
            {
                _logger.LogWarning("Motorcycle with license plate {LicensePlate} not found.", licensePlate);
                return NotFound();
            }

            _motorcycleService.UpdateMotorcycle(licensePlate, motorcycle);
            return NoContent();
        }

        [Authorize]
        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{licensePlate}")]
        public IActionResult Delete(string licensePlate)
        {
            _logger.LogInformation("Deleting motorcycle with license plate {LicensePlate}.", licensePlate);
            var existingMotorcycle = _motorcycleService.GetMotorcycleByLicensePlate(licensePlate);
            if (existingMotorcycle == null)
            {
                _logger.LogWarning("Motorcycle with license plate {LicensePlate} not found.", licensePlate);
                return NotFound();
            }

            _motorcycleService.DeleteMotorcycle(licensePlate);
            return NoContent();
        }
    }
}
