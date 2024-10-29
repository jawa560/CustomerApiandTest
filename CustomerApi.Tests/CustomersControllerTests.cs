using CustomerApi.Models;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Net.Http.Json;

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

            // 使用強類型來解析 JSON 響應
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenResponse.Token;
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
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateOnly(1990, 1, 1) },
                        new Customer { Id = 2, Name = "Jane Smith", Address = "456 Elm St", Phone = "555-5678", Birthday = new System.DateOnly(1985, 2, 2) }
                    };
                    services.AddScoped(_ => mockCustomers);

                    if (mockCustomers != null)
                    {
                        foreach (var customer in mockCustomers)
                        {
                            Debug.WriteLine($"Id: {customer.Id}, Name: {customer.Name}, Address: {customer.Address}, Phone: {customer.Phone}, Birthday: {customer.Birthday}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("mockCustomers list is null");
                    }
                });
            }).CreateClient();
           

            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.GetAsync("/api/customers");

            // Assert
            response.EnsureSuccessStatusCode();
            var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();

            // Print the customers list
            if (customers != null)
            {
                foreach (var customer in customers)
                {
                    Debug.WriteLine($"Id: {customer.Id}, Name: {customer.Name}, Address: {customer.Address}, Phone: {customer.Phone}, Birthday: {customer.Birthday}");
                }
            }
            else
            {
                Debug.WriteLine("Customers list is null");
            }
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
                    services.AddScoped(_ => mockCustomers);
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
                Birthday = new System.DateOnly(1995, 3, 3)
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
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateOnly(1990, 1, 1) }
                    };
                    services.AddScoped(_ => mockCustomers);
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
                Birthday = new System.DateOnly(1990, 1, 1)
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
                        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Birthday = new System.DateOnly(1990, 1, 1) }
                    };
                    services.AddScoped(_ => mockCustomers);
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

    // 定義 TokenResponse 類別
    public class TokenResponse
    {
        public string Token { get; set; }
    }
}

