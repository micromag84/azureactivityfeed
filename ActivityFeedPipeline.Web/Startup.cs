using ActivityFeedPipeline.Web.Core;
using ActivityFeedPipeline.Web.CosmosDb;
using ActivityFeedPipeline.Web.Storage;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace ActivityFeedPipeline.Web
{
	[UsedImplicitly]
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddOptions();
			services.Configure<DocumentClientOptions>(Configuration.GetSection("DocumentClient"));

			services.AddTransient<IDocumentTypedCollectionClient<ActivityFeedQueueItem>, DocumentTypedCollectionClient<ActivityFeedQueueItem>>();

			AddStorageQueue(services, Configuration);

			services.AddSingleton<IHostedService, ActivityItemsHostedService>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseMvc();

			app.ApplicationServices.GetService<IDocumentTypedCollectionClient<ActivityFeedQueueItem>>().Initialize();
		}

		private static void AddStorageQueue(IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("QueueStorageConnection");

			services.AddScoped<IStorageQueueClient, AzureStorageQueueClient>(provider =>
				new AzureStorageQueueClient(connectionString, provider.GetRequiredService<ILogger<AzureStorageQueueClient>>()));
		}
	}
}