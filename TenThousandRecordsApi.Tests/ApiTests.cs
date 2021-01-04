using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TenThousandRecordsApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace TenThousandRecordsApi.Tests
{
    public class ApiTests
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly ITestOutputHelper output;
        private readonly WebApplicationFactory<Startup> factory;

        public ApiTests(ITestOutputHelper output, WebApplicationFactory<Startup> factory)
        {
            this.output = output;
            this.factory = factory;
            
            var projectDir = Directory.GetCurrentDirectory();
            var testSettings = Path.Combine(projectDir, "appsettings.json");

            this.factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context,conf) =>
                {
                    conf.AddJsonFile(testSettings);
                });
            });
        }
        
        [Fact]
        public async Task InMemoryTest()
        {
            var client = factory.CreateClient();
            
            HttpResponseMessage response;
            using (new Timing("in-memory", output))
            {
                response = await client.GetAsync("/api/in-memory");
            }
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<Product>();
            Assert.NotEqual(0, product?.Id);
            output.WriteLine($"Product Name: {product?.Name}");
        }
        
        [Fact]
        public async Task SqliteTest()
        {
            var client = factory.CreateClient();
            HttpResponseMessage response;
            
            using (new Timing("sql", output))
            {
                response = await client.GetAsync("/api/sql");
            }
            
            
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<Product>();
            
            Assert.NotEqual(0, product?.Id);
            output.WriteLine($"Product Name: {product?.Name}");
        }

        public class Timing : IDisposable
        {
            private readonly string description;
            private readonly ITestOutputHelper output;
            private Stopwatch stopwatch;

            public Timing(string description, ITestOutputHelper output)
            {
                this.description = description;
                this.output = output;
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            public void Dispose()
            {
                stopwatch.Stop();
                output.WriteLine($"{description}: {stopwatch.Elapsed.TotalMilliseconds} ms.");
            }
        }
    }
}