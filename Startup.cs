using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevBin {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();

            // Pastes transformer
            services.AddTransient<PasteTransformer>();
            services.AddTransient<RawTransformer>();


            var mvc = services.AddMvc();
            mvc.AddRazorRuntimeCompilation();

            services.Add(new ServiceDescriptor(typeof(Database), new Database(Configuration.GetConnectionString("Database"))));
            services.Add(new ServiceDescriptor(typeof(PasteFs), new PasteFs(Configuration.GetValue<string>("DataPath"), Configuration.GetValue<bool>("UseCompression"))));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

            /*if ( env.IsDevelopment() ) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
            }*/

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapDynamicPageRoute<PasteTransformer>(@"{pasteId:regex(^[A-Za-z]{{8}})}");
                endpoints.MapDynamicPageRoute<RawTransformer>(@"raw/{pasteId:regex(^[A-Za-z{{8}}])}");
            });
        }
    }
}
