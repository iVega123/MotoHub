using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoHub.DTOs;
using MotoHub.Models;
using MotoHub.Repositories;
using MotoHub.Services;

namespace MotoHubTests.Unit.Services
{
    public class MotorcycleServiceTests
    {
        [Fact]
        public void GetAllMotorcycles_ReturnsAllMotorcycles()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2019 }
            };

            var motorcycleDTOs = new List<MotorcycleDTO>
            {
                new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new MotorcycleDTO { LicensePlate = "DEF456", Model = "Yamaha", Year = 2019 }
            };

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<IEnumerable<MotorcycleDTO>>(It.IsAny<IEnumerable<Motorcycle>>()))
                      .Returns(motorcycleDTOs);

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetAll())
                          .Returns(motorcycles);

            var service = new MotorcycleService(mockRepository.Object, mockMapper.Object);

            // Act
            var result = service.GetAllMotorcycles();

            // Assert
            Assert.NotNull(result);
            Assert.Collection(result,
                item => Assert.Equal("ABC123", item.LicensePlate),
                item => Assert.Equal("DEF456", item.LicensePlate)
            );
        }

        [Fact]
        public void GetMotorcycleByLicensePlate_ExistingLicensePlate_ReturnsMotorcycleDTO()
        {
            // Arrange
            var existingLicensePlate = "ABC123";
            var existingMotorcycle = new Motorcycle { Id = 1, LicensePlate = existingLicensePlate, Model = "Honda", Year = 2020 };
            var motorcycleDTO = new MotorcycleDTO { LicensePlate = existingLicensePlate, Model = "Honda", Year = 2020 };

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<MotorcycleDTO>(It.IsAny<Motorcycle>()))
                      .Returns(motorcycleDTO);

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetByLicensePlate(existingLicensePlate))
                          .Returns(existingMotorcycle);

            var service = new MotorcycleService(mockRepository.Object, mockMapper.Object);

            // Act
            var result = service.GetMotorcycleByLicensePlate(existingLicensePlate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingLicensePlate, result.LicensePlate);
        }

        [Fact]
        public void CreateMotorcycle_ValidMotorcycle_CreatesAndReturnsCreatedAtAction()
        {
            // Arrange
            var motorcycleDTO = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };
            var createdMotorcycle = new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<Motorcycle>(It.IsAny<MotorcycleDTO>()))
                      .Returns(createdMotorcycle);

            var mockRepository = new Mock<IMotorcycleRepository>();

            var service = new MotorcycleService(mockRepository.Object, mockMapper.Object);

            // Act
            service.CreateMotorcycle(motorcycleDTO);

            // Assert
            mockRepository.Verify(repo => repo.Add(It.IsAny<Motorcycle>()), Times.Once);
            mockMapper.Verify(m => m.Map<Motorcycle>(motorcycleDTO), Times.Once);
        }

        [Fact]
        public void UpdateMotorcycle_ExistingMotorcycle_UpdatesMotorcycle()
        {
            // Arrange
            var existingLicensePlate = "ABC123";
            var existingMotorcycle = new Motorcycle { Id = 1, LicensePlate = existingLicensePlate, Model = "Honda", Year = 2020 };
            var updatedMotorcycleDTO = new MotorcycleDTO { LicensePlate = existingLicensePlate, Model = "Yamaha", Year = 2021 };

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<MotorcycleDTO>(It.IsAny<Motorcycle>()))
                      .Returns(updatedMotorcycleDTO);

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetByLicensePlate(existingLicensePlate))
                          .Returns(existingMotorcycle);

            var service = new MotorcycleService(mockRepository.Object, mockMapper.Object);

            // Act
            service.UpdateMotorcycle(existingLicensePlate, updatedMotorcycleDTO);

            // Assert
            Assert.Equal("Yamaha", existingMotorcycle.Model);
            Assert.Equal(2021, existingMotorcycle.Year);
        }

        [Fact]
        public void DeleteMotorcycle_ExistingMotorcycle_DeletesMotorcycle()
        {
            // Arrange
            var existingLicensePlate = "ABC123";
            var existingMotorcycle = new Motorcycle { Id = 1, LicensePlate = existingLicensePlate, Model = "Honda", Year = 2020 };

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetByLicensePlate(existingLicensePlate))
                          .Returns(existingMotorcycle);

            var service = new MotorcycleService(mockRepository.Object, Mock.Of<IMapper>());

            // Act
            service.DeleteMotorcycle(existingLicensePlate);

            // Assert
            mockRepository.Verify(repo => repo.Delete(existingMotorcycle.Id), Times.Once);
        }

        [Fact]
        public void LicensePlateExists_ExistingLicensePlate_ReturnsTrue()
        {
            // Arrange
            var existingLicensePlate = "ABC123";

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.LicensePlateExists(existingLicensePlate))
                          .Returns(true);

            var service = new MotorcycleService(mockRepository.Object, Mock.Of<IMapper>());

            // Act
            var result = service.LicensePlateExists(existingLicensePlate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateMotorcycle_DuplicateLicensePlate_ReturnsConflict()
        {
            // Arrange
            var motorcycleDTO = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.LicensePlateExists(motorcycleDTO.LicensePlate))
                          .Returns(true);

            var service = new MotorcycleService(mockRepository.Object, Mock.Of<IMapper>());

            // Act
            var result = Record.Exception(() => service.CreateMotorcycle(motorcycleDTO));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void UpdateMotorcycle_NonExistingMotorcycle_DoesNotUpdate()
        {
            // Arrange
            var nonExistingLicensePlate = "XYZ789";
            var updatedMotorcycleDTO = new MotorcycleDTO { LicensePlate = nonExistingLicensePlate, Model = "Yamaha", Year = 2021 };

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetByLicensePlate(nonExistingLicensePlate))
                          .Returns((Motorcycle)null);

            var service = new MotorcycleService(mockRepository.Object, Mock.Of<IMapper>());

            // Act
            service.UpdateMotorcycle(nonExistingLicensePlate, updatedMotorcycleDTO);

            // Assert
            mockRepository.Verify(repo => repo.Update(It.IsAny<Motorcycle>()), Times.Never);
        }

        [Fact]
        public void DeleteMotorcycle_NonExistingMotorcycle_DoesNotDelete()
        {
            // Arrange
            var nonExistingLicensePlate = "XYZ789";

            var mockRepository = new Mock<IMotorcycleRepository>();
            mockRepository.Setup(repo => repo.GetByLicensePlate(nonExistingLicensePlate))
                          .Returns((Motorcycle)null);

            var service = new MotorcycleService(mockRepository.Object, Mock.Of<IMapper>());

            // Act
            service.DeleteMotorcycle(nonExistingLicensePlate);

            // Assert
            mockRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
        }
    }
}
