using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MotoHub.Controllers;
using MotoHub.DTOs;
using MotoHub.Services;
using System.Security.Claims;

namespace MotoHubTests.Unit.Controllers
{
    public class MotorcyclesControllerTests
    {
        [Fact]
        public void GetAll_ReturnsOkObjectResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetAllMotorcycles()).Returns(new[] { new MotorcycleDTO() { LicensePlate = "test-584" } });
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.GetAll();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetByLicensePlate_WithExistingPlate_ReturnsOkObjectResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetMotorcycleByLicensePlate("ABC123")).Returns(new MotorcycleDTO() { LicensePlate = "test-584" });
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.GetByLicensePlate("ABC123");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Create_ValidMotorcycle_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.LicensePlateExists("ABC123")).Returns(false);
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);
            var motorcycle = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            // Act
            var result = controller.Create(motorcycle);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public void Update_WithExistingLicensePlate_ReturnsNoContentResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetMotorcycleByLicensePlate("ABC123")).Returns(new MotorcycleDTO() { LicensePlate = "test-584" });
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.Update("ABC123", new MotorcycleDTO() { LicensePlate = "A" });

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_WithExistingLicensePlate_ReturnsNoContentResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetMotorcycleByLicensePlate("ABC123")).Returns(new MotorcycleDTO() { LicensePlate = "test-584" });
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.Delete("ABC123");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Create_DuplicateLicensePlate_ReturnsConflictResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.LicensePlateExists("ABC123")).Returns(true);
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);
            var motorcycle = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            // Act
            var result = controller.Create(motorcycle);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public void Update_NonExistingLicensePlate_ReturnsNotFoundResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetMotorcycleByLicensePlate("XYZ789")).Returns((MotorcycleDTO)null);
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.Update("XYZ789", new MotorcycleDTO() { LicensePlate = "test-584" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_NonExistingLicensePlate_ReturnsNotFoundResult()
        {
            // Arrange
            var motorcycleServiceMock = new Mock<IMotorcycleService>();
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            motorcycleServiceMock.Setup(service => service.GetMotorcycleByLicensePlate("XYZ789")).Returns((MotorcycleDTO)null);
            var controller = new MotorcyclesController(motorcycleServiceMock.Object, mockLogger.Object);

            // Act
            var result = controller.Delete("XYZ789");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetAll_AuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var controller = new MotorcyclesController(Mock.Of<IMotorcycleService>(), Mock.Of<ILogger<MotorcyclesController>>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "TestUser"),
                        new Claim(ClaimTypes.Email, "test@example.com"),
                        new Claim(ClaimTypes.Role, "Admin")
                    }, "mock"))
                }
            };

            // Act
            var result = controller.GetAll();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetByLicensePlate_AuthorizedUser_ReturnsOkResult()
        {
            // Arrange
            var mockService = new Mock<IMotorcycleService>();
            mockService.Setup(service => service.GetMotorcycleByLicensePlate(It.IsAny<string>())).Returns(new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 });
            var mockLogger = new Mock<ILogger<MotorcyclesController>>();
            var controller = new MotorcyclesController(mockService.Object, mockLogger.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "TestUser"),
                        new Claim(ClaimTypes.Email, "test@example.com"),
                        new Claim(ClaimTypes.Role, "Admin")
                    }, "mock"))
                }
            };

            // Act
            var result = controller.GetByLicensePlate("ABC123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<MotorcycleDTO>(okResult.Value);
            Assert.Equal("ABC123", model.LicensePlate);
        }
    }
}
