using WebApp.Data.Extensions;
using WebApp.Extensions;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using TenantManagement.Extensions;
using TenantManagement.Common;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.HttpLogging;

namespace WebApp
{
    public class Startup
    {
        private readonly string APPGLOBAL_CONNECTIONSTRING_NAME = "WebAppDatabase";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ReactFrontend/build";
            });

            var connectionString = Configuration.GetConnectionString(APPGLOBAL_CONNECTIONSTRING_NAME);
            services.RegisterAppGlobalDBContexts(connectionString);
            services.AddControllersWithViews().AddNewtonsoftJson();

            services.RegisterTenantRepositories();
            services.RegisterTenantServices();
            services.RegisterTenantControllers();

            services.RegisterRepositories();
            services.RegisterServices();
            services.RegisterControllers();

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.ConstraintMap.Add("email", typeof(EmailConstraint));
            });

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type =>
                   type.ToString()
                );

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "WebApp",
                    Description = "Web API Application",
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                var XmlCommentsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Assembly.GetExecutingAssembly().GetName().Name + ".xml");
                if (File.Exists(XmlCommentsFilePath))
                {
                    c.IncludeXmlComments(XmlCommentsFilePath);
                    c.IncludeXmlComments(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TenantManagement.xml"));
                }
            });

            services.AddCors(options =>
            {
                options.AddPolicy("FMCorsPolicy", builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                // NOTE: code below will add the UTC timezone to datetime - however not needed since mostly working with dates
                //options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                //options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
            });

            // NOTE: Uncomment for Custom JWT Management
            var secret = Encoding.ASCII.GetBytes(Configuration["AppSecret"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = Configuration["JwtConfig:Issuer"],
                    ValidAudience = Configuration["JwtConfig:Audience"],
                    ClockSkew = TimeSpan.Zero,
                };
            });

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var scopeFactory = services
                               .BuildServiceProvider()
                               .GetRequiredService<IServiceScopeFactory>();

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .UseActivator(new CustomHangfireJobActivator(scopeFactory))
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString(APPGLOBAL_CONNECTIONSTRING_NAME), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();
            services.AddFeatureManagement();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("Local") || env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "swagger/{documentname}/swagger.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web App API V1");
                    c.RoutePrefix = "swagger";
                    c.DocExpansion(DocExpansion.None);
                    c.DefaultModelRendering(ModelRendering.Model);
                    c.DefaultModelExpandDepth(1);
                    c.EnableTryItOutByDefault();
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            GlobalConfiguration.Configuration.UseFilter(new BackgroundJobFilter());

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = Configuration.GetSection(nameof(Hangfire)).GetValue(typeof(string), "AdminUser").ToString(),
                            PasswordClear =  Configuration.GetSection(nameof(Hangfire)).GetValue(typeof(string), "BasicAuthPass").ToString()
                        },
                    }
                }) },
                IgnoreAntiforgeryToken = true
            });

            app.UseRouting();
            app.UseCors("FMCorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpLogging();

            app.UseMiddleware<HttpHeadersMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            // NOTE: Uncomment code for running React Server during development
            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ReactFrontend";
            //    if (env.IsEnvironment("Local") || env.IsDevelopment())
            //    {
            //        spa.UseReactDevelopmentServer(npmScript: "start");
            //    }
            //});
        }

        private String GetSettingsValue(String key)
        {
            if (!String.IsNullOrEmpty(Configuration[key]))
            {
                return Configuration[key];
            }

            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                return Environment.GetEnvironmentVariable(key);
            }

            throw new ArgumentException($"Unable to access environment variable: {key}");
        }
    }
}