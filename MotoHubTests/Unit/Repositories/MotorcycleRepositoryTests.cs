using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using MotoHub.Data;
using MotoHub.Models;
using MotoHub.Repositories;

namespace MotoHubTests.Unit.Repositories
{
    public class MotorcycleRepositoryTests
    {
        [Fact]
        public void GetAll_ReturnsAllMotorcycles()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles).ReturnsDbSet(motorcycles);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.GetAll();

            // Assert
            Assert.NotNull(result);
            var returnedMotorcycles = result.ToList();
            Assert.Equal(2, returnedMotorcycles.Count());
            Assert.Collection(returnedMotorcycles,
                item => Assert.Equal("ABC123", item.LicensePlate),
                item => Assert.Equal("DEF456", item.LicensePlate)
            );
        }

        [Fact]
        public void GetById_ReturnsCorrectMotorcycle()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles.Find(1)).Returns(motorcycles[0]);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ABC123", result.LicensePlate);
        }

        [Fact]
        public void Add_AddsNewMotorcycle()
        {
            // Arrange
            var motorcycleToAdd = new Motorcycle { Id = 3, LicensePlate = "GHI789", Model = "Suzuki", Year = 2022 };

            var mockContext = new Mock<IApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Motorcycle>>();
            mockContext.Setup(c => c.Motorcycles).Returns(mockDbSet.Object);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            repository.Add(motorcycleToAdd);

            // Assert
            mockDbSet.Verify(dbSet => dbSet.Add(motorcycleToAdd), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }


        [Fact]
        public void LicensePlateExists_ReturnsTrue_WhenLicensePlateExists()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles).ReturnsDbSet(motorcycles);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.LicensePlateExists("ABC123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void LicensePlateExists_ReturnsFalse_WhenLicensePlateDoesNotExist()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles).ReturnsDbSet(motorcycles);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.LicensePlateExists("GHI789");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetByLicensePlate_ReturnsCorrectMotorcycle_WhenLicensePlateExists()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles).ReturnsDbSet(motorcycles);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.GetByLicensePlate("ABC123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ABC123", result.LicensePlate);
        }

        [Fact]
        public void GetById_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = 1, LicensePlate = "ABC123", Model = "Honda", Year = 2020 },
                new Motorcycle { Id = 2, LicensePlate = "DEF456", Model = "Yamaha", Year = 2021 }
            };

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Motorcycles.Find(3)).Returns(() => null);

            var repository = new MotorcycleRepository(mockContext.Object);

            // Act
            var result = repository.GetById(3);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Add_ThrowsException_WhenMotorcycleIsNull()
        {
            // Arrange
            var mockContext = new Mock<IApplicationDbContext>();
            var repository = new MotorcycleRepository(mockContext.Object);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => repository.Add(null));
        }
    }
}
