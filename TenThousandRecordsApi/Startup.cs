using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TenThousandRecordsApi.Models;

namespace TenThousandRecordsApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Database");

            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<Database>(o => o.UseSqlite(connectionString));
            
            // load all products into memory
            using var db = new Database(
                new DbContextOptionsBuilder<Database>()
                    .UseSqlite(connectionString)
                    .Options
            ); 
            
            var inMemory = new InMemory(db.Products.AsNoTracking().ToList());
            services.AddSingleton(inMemory);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(
                        @"<html>
                            <body>
                                <h1>To Query Or Cache?</h1>
                                <ul>
                                    <li><a href='/api/in-memory'>In Memory</a></li>
                                    <li><a href='/api/sql'>Sql</a></li>
                                </ul>
                            </body>
                          </html>                                    
                        ");
                });
                
                endpoints.MapGet("/api/in-memory", async context =>
                {
                    var random = new Random();
                    var db = context.RequestServices.GetRequiredService<InMemory>();
                    var id = random.Next(1, 10_000);
                    var product = db.Products.First(p => p.Id == id);

                    await context.Response.WriteAsJsonAsync(product);
                });
                
                endpoints.MapGet("/api/sql", async context =>
                {
                    var random = new Random();
                    var db = context.RequestServices.GetRequiredService<Database>();

                    var id = random.Next(1, 10_000);
                    var product = await db
                        .Products
                        .AsNoTracking()
                        .FirstAsync(p => p.Id == id);

                    await context.Response.WriteAsJsonAsync(product);
                });
            });
        }
    }
}