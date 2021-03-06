using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Interfaces;

namespace DevBin {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDistributedMemoryCache();

            services.AddMemoryCache();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromSeconds(300);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "devbin_session";
            });

            services.AddRazorPages();

            // Pastes transformer
            services.AddTransient<PasteTransformer>();
            services.AddTransient<RawTransformer>();


            var mvc = services.AddMvc();
            mvc.AddRazorRuntimeCompilation();

            services.Add(new ServiceDescriptor(typeof(Database),
                new Database(Configuration.GetConnectionString("Database"))));
            services.Add(new ServiceDescriptor(typeof(PasteFs),
                new PasteFs(Configuration.GetValue<string>("DataPath"),
                    Configuration.GetValue<bool>("UseCompression"))));

            services.Configure<ClientRateLimitOptions>(options => {
                options.GeneralRules = new List<RateLimitRule> {
                    new() {
                        Endpoint = "/Index",
                        Period = "1m",
                        Limit = 5
                    }
                };
            });

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options => {
                    options.InvalidModelStateResponseFactory = context => {
                        var result = new BadRequestObjectResult(context.ModelState);

                        var errors = context.ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .Select(x => new {x.Key, x.Value.Errors})
                            .ToArray();

                        var fullErrors = new StringBuilder();

                        foreach (var error in errors)
                        foreach (var message in error.Errors)
                            fullErrors.Append(message.ErrorMessage);

                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        result.Value = new API.Response(400, fullErrors.ToString());
                        return result;
                    };
                })
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v2", new OpenApiInfo {
                    Title = "DevBin",
                    Version = "v2",
                    Description = "Fetch and create pastes with the DevBin API",
                    Contact = new OpenApiContact {
                        Name = "AlexDevs",
                        Url = new Uri("https://alexdevs.pw")
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
            app.UseHsts();


            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
            }
            
            app.Use((context, next) => {
                // Create a session identifier based on the user agent, ip address and current day
                var sessionIdByte =
                    SHA256.HashData(Encoding.ASCII.GetBytes(
                        context.Request.Headers["User-Agent"] +
                        context.Connection.RemoteIpAddress +
                        DateTime.Today
                    ));

                string sessionId = BitConverter.ToString(sessionIdByte).Replace("-", "");
                context.Items.TryAdd("session_id", sessionId);

                if (!context.Request.Cookies.TryGetValue("devbin_session_token", out var token)) return next();
                if (string.IsNullOrEmpty(token)) return next();
                var user = Database.Instance.ResolveSessionToken(token);
                if (user != null) {
                    context.Items.TryAdd("logged_user", user);
                }

                return next();
            });

            app.UseSwagger(c => { c.RouteTemplate = "docs/{documentname}/swagger.json"; });
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/docs/v2/swagger.json", "DevBin API");
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.DocumentTitle = "DevBin API Documentation";
                c.RoutePrefix = "docs";
            });

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapDynamicPageRoute<PasteTransformer>(@"{pasteId:regex(^[A-Za-z]{{8}})}");
                endpoints.MapDynamicPageRoute<RawTransformer>(@"raw/{pasteId:regex(^[A-Za-z{{8}}])}");
            });
        }
    }
}