using System;
using ActivityFeedPipeline.Web.Core;
using ActivityFeedPipeline.Web.CosmosDb;
using ActivityFeedPipeline.Web.Infrastructure;
using ActivityFeedPipeline.Web.Storage;
using JetBrains.Annotations;
using MediatR;
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
			services.AddMediatR();

			services.AddTransient<IDocumentTypedCollectionClient<ActivityFeedQueueItem>, DocumentTypedCollectionClient<ActivityFeedQueueItem>>();
			AddStorageQueue(services, Configuration);
			AddHostedServices(services);
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

			InitializeDocumentCollections(app.ApplicationServices);
		}

		private static void AddStorageQueue(IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("QueueStorageConnection");

			services.AddScoped<IStorageQueueClient, AzureStorageQueueClient>(provider =>
				new AzureStorageQueueClient(connectionString, provider.GetRequiredService<ILogger<AzureStorageQueueClient>>()));
		}

		private static void AddHostedServices(IServiceCollection services)
		{
			void AddMediatorTriggerHostedService<T>(TimeSpan delayInterval) where T : IRequest, new()
			{
				services.AddSingleton<IHostedService, MediatorTriggerHostedService<T>>(provider => new MediatorTriggerHostedService<T>(
					provider.GetService<IServiceScopeFactory>(),
					provider.GetService<ILogger<MediatorTriggerHostedService<T>>>(),
					delayInterval));
			}

			AddMediatorTriggerHostedService<HandleActivityItemsCommand>(TimeSpan.FromSeconds(10));
		}

		private static void InitializeDocumentCollections(IServiceProvider serviceProvider)
		{
			serviceProvider.GetService<IDocumentTypedCollectionClient<ActivityFeedQueueItem>>().Initialize();
		}
	}
}