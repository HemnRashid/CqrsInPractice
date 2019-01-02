using Api.Dtos;
using Api.Utils;
using Logic;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton(new SessionFactory(Configuration["ConnectionString"]));
            services.AddTransient<UnitOfWork>(); // using AddTransient becuasee our class dosnt have an dispose method.
            services.AddTransient<ICommandHandler<EditPersonalInfoCommand>, EditPersonalInfoCommandHandler>();
            services.AddSingleton<Messages>(); // bör endast en sån klass existera i vår applikation.
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
            app.UseMvc();
        }
    }
}
