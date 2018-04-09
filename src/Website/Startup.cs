using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Website.Utility;

namespace Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static Dictionary<string, string> LanguageExtentionConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            SolrUrl.SOLR_URL = Configuration.GetSection("AppSettings").Get<AppSettings>().SOLR_URL;
            SolrUrl.SOLR_USER = Configuration.GetSection("AppSettings").Get<AppSettings>().SOLR_USER;
            SolrUrl.SOLR_SECRET = Configuration.GetSection("AppSettings").Get<AppSettings>().SOLR_SECRET;

            LanguageExtentionConfig =
                Configuration.GetSection("LanguageExtentionConfig").GetChildren()
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value))
                .ToDictionary(y => y.Key, y => y.Value);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
