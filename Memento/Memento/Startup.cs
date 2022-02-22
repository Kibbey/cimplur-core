using Domain.Emails;
using Domain.Models;
using Domain.Repository;
using Memento.Libs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace Memento
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static System.IO.TextWriter Out { get; }

        public IConfiguration Configuration { get; }

        private string _policyFyli = "fyli_policy";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(opt =>
            {
                opt.AddPolicy(name: _policyFyli, builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            Console.Out.WriteLine("Hello world!");
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddScoped<UserWebToken, UserWebToken>();
            services.AddScoped<SendEmailService, SendEmailService>();
            services.AddScoped<NotificationService, NotificationService>();
            services.AddScoped<DropsService, DropsService>();
            services.AddScoped<SharingService, SharingService>();
            services.AddScoped<AlbumService, AlbumService>();
            services.AddScoped<PermissionService, PermissionService>();
            services.AddScoped<GroupService, GroupService>();
            services.AddScoped<MovieService, MovieService>();
            services.AddScoped<PromptService, PromptService>();
            services.AddScoped<ImageService, ImageService>();
            services.AddScoped<TimelineService, TimelineService>();
            services.AddScoped<UserService, UserService>();
            services.AddScoped<PlanService, PlanService>();
            services.AddScoped<ContactService, ContactService>();
            services.AddScoped<TokenService, TokenService>();
            services.AddControllers().AddNewtonsoftJson(); ;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Memento", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Memento v1"));
            }
            app.UseExceptionHandler(err => err.UseCustomErrors(env));
            app.UseHttpsRedirection();
            app.UseMiddleware<AuthMiddleWare>();
            app.UseRouting();

         
            app.UseCors(_policyFyli);
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
