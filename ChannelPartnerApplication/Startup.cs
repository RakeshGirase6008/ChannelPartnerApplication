using ChannelPartnerApplication.ActionFilter;
using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Factory;
using ChannelPartnerApplication.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChannelPartnerApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // Add the whole configuration object here.
            services.AddSingleton<IConfiguration>(Configuration);

            #region Inject Database

            services.AddDbContext<ClassBookManagementContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClassBookManagementeDatabase")));

            services.AddDbContext<ChannelPartnerManagementContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ChannelPartnerDatabase")));

            #endregion

            #region Inject Filters
            services.AddScoped<ControllerFilterExample>();
            #endregion

            #region Inject Services

            services.AddTransient<FileService, FileService>();
            services.AddTransient<ChannelPartnerService, ChannelPartnerService>();
            services.AddTransient<NotificationService, NotificationService>();
            services.AddTransient<LogsService, LogsService>();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ChannelPartnerModelFactory, ChannelPartnerModelFactory>();
            #endregion

            #region Version Management

            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });

            #endregion

            #region Compatibality Configuration
            // Compatiblity version set to Dot Net Core 3.0
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            #endregion

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(name: "v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "ChannelPartnerAPI", Version = "v1" });
                c.OperationFilter<CustomHeaderSwaggerAttribute>();
            });

            #endregion
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            #region Swagger settings

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "ChannelPartnerAPI v1");
            });

            #endregion
        }
    }
}
