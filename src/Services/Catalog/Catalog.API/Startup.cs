using Catalog.API.Models;
using MassTransit;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace Catalog.API
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
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["ConnectionString"],
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    }));

            services.AddTransient<IPlateRepository, PlateRepository>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "eShopOnContainers - Catalog HTTP API",
                    Version = "v1",
                    Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample"
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddControllers();
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddMassTransit(x =>
            {
                //x.AddConsumer<ConsumerClass>();

                //ADD CONSUMERS HERE
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["EventBusConnection"], "/", h =>
                    {
                        if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                        {
                            h.Username(Configuration["EventBusUserName"]);
                        }

                        if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                        {
                            h.Password(Configuration["EventBusPassword"]);
                        }
                    });

                    cfg.ConfigureEndpoints(context);
                    cfg.ExchangeType = ExchangeType.Fanout;
                });
            });

            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            app.UseStaticFiles();

            // Make work identity server redirections in Edge and lastest versions of browers. WARN: Not valid in a production environment.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'");
                await next();
            });

            app.UseForwardedHeaders();

            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Catalog.API V1");
                });

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
            });
        }
    }
}
