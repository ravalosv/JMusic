using JMusic.Data;
using JMusic.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JMusic.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));

            services.AddControllers(config => {
                config.ReturnHttpNotAcceptable = true;      // restringe a aceptar solo solicitudes con el formato configurado en el webapi (json y xml)
            }).AddXmlDataContractSerializerFormatters()   // Habilitar formato de comunicación XML
                ;

            services.AddDbContext<TiendaDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TiendaDb"))
            );

            services.ConfigureDependencies();
            services.ConfigureCors();
            services.ConfigureJwt(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseHsts();
            }

            app.UseRouting();
            
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
