using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMusic.Data;
using JMusic.Data.Contratos;
using JMusic.Data.Repositorios;
using JMusic.Models;
using JMusic.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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

            services.AddControllers();

            services.AddDbContext<TiendaDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TiendaDb"))
            );

            services.AddScoped<IProductosRepositorio, ProductosRepositorio>();
            services.AddScoped<IRepositorioGenerico<Perfil>, RepositorioPerfiles>();
            services.AddScoped<IOrdenesRepositorio, RepositorioOrdenes>();
            services.AddScoped<IUsuariosRepositorio, RepositorioUsuarios>();
            services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
            services.AddSingleton<TokenService>();


            //Accedemos a la sección JwtSettings del archivo appsettings.json
            var jwtSettings = Configuration.GetSection("JwtSettings");
            //Obtenemos la clave secreta guardada en JwtSettings:SecretKey
            string secretKey = jwtSettings.GetValue<string>("SecretKey");
            //Obtenemos el tiempo de vida en minutos del Jwt guardada en JwtSettings:MinutesToExpiration
            int minutes = jwtSettings.GetValue<int>("MinutesToExpiration");
            //Obtenemos el valor del emisor del token en JwtSettings:Issuer
            string issuer = jwtSettings.GetValue<string>("Issuer");
            //Obtenemos el valor de la audiencia a la que está destinado el Jwt en JwtSettings:Audience
            string audience = jwtSettings.GetValue<string>("Audience");

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(minutes)
                };
            });


            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    //.AllowCredentials()   //<- Marca error: The CORS protocol does not allow specifying a wildcard (any) origin and credentials 
                    // at the same time. Configure the CORS policy by listing individual origins if credentials needs to be 
                    // supported
                    );


                //    options.AddPolicy("CorsPolicy",
                //        builder => builder.WithOrigins("https://localhost")
                //        .AllowAnyMethod()
                //        .AllowAnyHeader()
                //        .AllowCredentials()
                //        );
                });
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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseAuthentication();
            app.UseCors("CorsPolicy");
        }
    }
}
