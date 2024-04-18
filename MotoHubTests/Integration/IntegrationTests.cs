using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MotoHub.DTOs;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MotoHubTests.Integration
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IConfiguration _configuration;

        public IntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [Fact]
        public async Task GetAll_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/motorcycles");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_ValidMotorcycleWithToken_ReturnsCreatedAtAction()
        {
            // Arrange
            var client = _factory.CreateClient();
            var motorcycle = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            var token = GenerateJwtToken();

            // Add the token to the HTTP headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(motorcycle), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/motorcycles", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("ABC123", responseContent); // Assuming the response contains the license plate
        }

        [Fact]
        public async Task GetByLicensePlate_ExistingPlate_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "ABC123";
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.GetAsync($"/api/motorcycles/{licensePlate}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByLicensePlate_NonExistingPlate_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "NonExisting";
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.GetAsync($"/api/motorcycles/{licensePlate}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_ExistingPlate_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "ABC123";
            var motorcycle = new MotorcycleDTO { LicensePlate = licensePlate, Model = "Honda", Year = 2020 };
            var updatedMotorcycle = new MotorcycleDTO { LicensePlate = licensePlate, Model = "UpdatedModel", Year = 2021 };

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(updatedMotorcycle), Encoding.UTF8, "application/json");
            var contentCreate = new StringContent(JsonConvert.SerializeObject(motorcycle), Encoding.UTF8, "application/json");
            var responseCreate = await client.PostAsync("/api/motorcycles", contentCreate);
            var response = await client.PutAsync($"/api/motorcycles/{licensePlate}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_NonExistingPlate_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "NonExisting";
            var updatedMotorcycle = new MotorcycleDTO { LicensePlate = licensePlate, Model = "UpdatedModel", Year = 2021 };

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(updatedMotorcycle), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/motorcycles/{licensePlate}", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ExistingPlate_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "ABC123";

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.DeleteAsync($"/api/motorcycles/{licensePlate}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingPlate_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "NonExisting";

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.DeleteAsync($"/api/motorcycles/{licensePlate}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_MissingToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var motorcycle = new MotorcycleDTO { LicensePlate = "ABC123", Model = "Honda", Year = 2020 };

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(motorcycle), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/motorcycles", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_MissingToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "ABC123";
            var updatedMotorcycle = new MotorcycleDTO { LicensePlate = licensePlate, Model = "UpdatedModel", Year = 2021 };

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(updatedMotorcycle), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/motorcycles/{licensePlate}", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Delete_MissingToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var licensePlate = "ABC123";

            // Act
            var response = await client.DeleteAsync($"/api/motorcycles/{licensePlate}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_DuplicateLicensePlate_ReturnsConflict()
        {
            // Arrange
            var client = _factory.CreateClient();
            var motorcycle = new MotorcycleDTO { LicensePlate = "ExistingPlate", Model = "Honda", Year = 2020 };

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(motorcycle), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/motorcycles", content);
            var response = await client.PostAsync("/api/motorcycles", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }


        private string GenerateJwtToken()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
                // Add more claims as needed
            };

            var jwtKey = _configuration["JwtKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
