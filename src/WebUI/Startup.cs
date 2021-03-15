using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using QuRest.Application;
using QuRest.Application.Examples;
using QuRest.Application.Interfaces;
using QuRest.Infrastructure;
using QuRest.WebUI.OpenApi;
using System;
using System.IO;
using System.Reflection;

namespace QuRest.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure(this.Configuration);

            services.AddControllers();

            services.AddMvcCore()
                .AddApiExplorer()
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));

            services.AddFeatureManagement();

            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "QuRest",
                    Version = "v1",
                    Description = "A REST-full Approach for Hybrid Quantum-Classical Circuit Modeling",
                    TermsOfService = new Uri("https://github.com/StuttgarterDotNet/qurest"),
                    Contact = new OpenApiContact
                    {
                        Name = "Daniel Fink",
                        Url = new Uri("https://github.com/StuttgarterDotNet")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under Apache 2.0",
                        Url = new Uri("https://opensource.org/licenses/Apache-2.0")
                    }
                });

                c.EnableAnnotations();
                c.DocumentFilter<FeatureGateFilter>();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuRest v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (!this.Configuration.GetValue<bool>("UseInMemoryDatabase")) return;

            var db = app.ApplicationServices.GetService<IApplicationDbContext>();
            if (db == null) return;

            foreach (var circuit in QuantumCircuitExamples.All)
            {
                db.QuantumCircuits.CreateAsync(circuit);
            }
        }
    }
}
