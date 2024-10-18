using CustomerApi.Controllers;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CustomerApi.Tests
{
    public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CustomersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private async Task<string> GetJwtToken(string username, string password)
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
            response.EnsureSuccessStatusCode();
            var tokenResponse = await response.Content.ReadFromJsonAsync<dynamic>();
            return tokenResponse.token;
        }

        [Fact]
        public async Task Get_ReturnsAllCustomers()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mock the data
                    var mockCustomers = new List<Customer>
                    {
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateTime(1990, 1, 1) },
                        new Customer { Id = 2, Name = "Jane Smith", Address = "456 Elm St", Phone = "555-5678", Birthday = new System.DateTime(1985, 2, 2) }
                    };
                    services.AddSingleton(mockCustomers);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/customers");

            // Assert
            response.EnsureSuccessStatusCode();
            var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
            Assert.Equal(2, customers.Count);
        }

        [Fact]
        public async Task Post_CreatesNewCustomer()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mock the data
                    var mockCustomers = new List<Customer>();
                    services.AddSingleton(mockCustomers);
                });
            }).CreateClient();

            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var newCustomer = new Customer
            {
                Id = 3,
                Name = "New Customer",
                Address = "789 Oak St",
                Phone = "555-9876",
                Birthday = new System.DateTime(1995, 3, 3)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/customers", newCustomer);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdCustomer = await response.Content.ReadFromJsonAsync<Customer>();
            Assert.Equal(newCustomer.Id, createdCustomer.Id);
            Assert.Equal(newCustomer.Name, createdCustomer.Name);
            Assert.Equal(newCustomer.Address, createdCustomer.Address);
            Assert.Equal(newCustomer.Phone, createdCustomer.Phone);
            Assert.Equal(newCustomer.Birthday, createdCustomer.Birthday);
        }

        [Fact]
        public async Task Put_UpdatesCustomer()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mock the data
                    var mockCustomers = new List<Customer>
                    {
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateTime(1990, 1, 1) }
                    };
                    services.AddSingleton(mockCustomers);
                });
            }).CreateClient();

            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updatedCustomer = new Customer
            {
                Id = 1,
                Name = "Updated Name",
                Address = "Updated Address",
                Phone = "Updated Phone",
                Birthday = new System.DateTime(1990, 1, 1)
            };

            // Act
            var response = await client.PutAsJsonAsync("/api/customers/1", updatedCustomer);

            // Assert
            response.EnsureSuccessStatusCode();
            var customers = await client.GetFromJsonAsync<List<Customer>>("/api/customers");
            var customer = customers.Find(c => c.Id == 1);
            Assert.Equal(updatedCustomer.Name, customer.Name);
            Assert.Equal(updatedCustomer.Address, customer.Address);
            Assert.Equal(updatedCustomer.Phone, customer.Phone);
        }

        [Fact]
        public async Task Delete_RemovesCustomer()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mock the data
                    var mockCustomers = new List<Customer>
                    {
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateTime(1990, 1, 1) }
                    };
                    services.AddSingleton(mockCustomers);
                });
            }).CreateClient();

            var token = await GetJwtToken("admin", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.DeleteAsync("/api/customers/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var customers = await client.GetFromJsonAsync<List<Customer>>("/api/customers");
            Assert.Empty(customers);
        }
    }
}
