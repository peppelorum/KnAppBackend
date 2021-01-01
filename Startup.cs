using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piranha;
using Piranha.AttributeBuilder;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Data.EF.SQLServer;
using Piranha.AspNetCore.Identity.SQLServer;
using Azure.Storage.Blobs;
using Services;
using KnApp.Services;

namespace KnApp
{
    public class Startup
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configuration">The current configuration</param>
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Service setup
            services.AddPiranha(options =>
            {
                options.AddRazorRuntimeCompilation = true;

                options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
                options.UseImageSharp();
                options.UseManager();
                options.UseTinyMCE();
                options.UseMemoryCache();
                // options.UseEF<SQLiteDb>(db =>
                //     db.UseSqlite(_config.GetConnectionString("sqlite")));
                // options.UseIdentityWithSeed<IdentitySQLiteDb>(db =>
                //     db.UseSqlite(_config.GetConnectionString("sqlite")));

                // services.AddDbContext<GeoDb>(options => options.UseSqlite(_config.GetConnectionString("sqlite")));

                // services.AddMvc().AddJsonOptions(options => {
                //     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                // });


                services.AddControllers().AddNewtonsoftJson();
                services.AddApplicationInsightsTelemetry();

                services.AddMvc();
                services.AddSingleton<IConfiguration>(_config);  

                services.AddScoped(x => new BlobServiceClient(_config.GetValue<string>("BlobStorage:Connectionstring")));
                services.AddScoped<IBlobService, BlobService>();
                services.AddScoped<IEmailService, EmailService>();

                // services.AddTransient<ItemsController>();

                // services.AddSingleton<IConfiguration>(Configuration);



                options.UseEF<SQLServerDb> (db =>
                    db.UseSqlServer(_config.GetConnectionString("piranha"))
                );
                options.UseIdentityWithSeed<IdentitySQLServerDb>(db =>
                    db.UseSqlServer(_config.GetConnectionString("piranha")));

                services.AddDbContext<GeoDb>(options => options.UseSqlServer(_config.GetConnectionString("piranha"), x => x.UseNetTopologySuite()));

                services.AddDbContext<TokenDb>(options => options.UseSqlServer(_config.GetConnectionString("piranha")));

                // services.AddDbContext<MyAppDbContext>(options => {
                //     options.UseSqlServer(
                //         Configuration.GetConnectionString("DefaultConnection"));
                // }, ServiceLifetime.Transient);

                /***
                 * Here you can configure the different permissions
                 * that you want to use for securing content in the
                 * application.
                options.UseSecurity(o =>
                {
                    o.UsePermission("WebUser", "Web User");
                });
                 */
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApi api)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Initialize Piranha
            App.Init(api);

            // Build content types
            new ContentTypeBuilder(api)
                .AddAssembly(typeof(Startup).Assembly)
                .Build()
                .DeleteOrphans();

            // Configure Tiny MCE
            EditorConfig.FromFile("editorconfig.json");

            // Middleware setup
            app.UsePiranha(options => {
                options.UseManager();
                options.UseTinyMCE();
                options.UseIdentity();
            });
        }
    }
}
