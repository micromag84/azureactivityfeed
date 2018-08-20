using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ActivityFeedPipeline.Web.Infrastructure
{
	[UsedImplicitly]
	public class MediatorTriggerHostedService<T> : BackgroundService where T : IRequest, new()
	{
		private readonly TimeSpan delayInterval;
		private readonly ILogger<MediatorTriggerHostedService<T>> logger;
		private readonly IServiceScopeFactory scopeFactory;
		private DateTime lastTimeILogged = DateTime.MinValue;

		public MediatorTriggerHostedService(
			[NotNull] IServiceScopeFactory scopeFactory,
			[NotNull] ILogger<MediatorTriggerHostedService<T>> logger,
			TimeSpan delayInterval)
		{
			this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.delayInterval = delayInterval;

			if (delayInterval.Ticks == 0)
			{
				throw new ArgumentException("Delay cannot have 0 ticks.", nameof(delayInterval));
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				LogThatYoureStillAround();
				try
				{
					using (var scope = scopeFactory.CreateScope())
					{
						var mediator = scope.ServiceProvider.GetService<IMediator>();

						// wait at leat delay or longer if the publish takes longer.
						await Task.WhenAll(
							mediator.Send(new T(), stoppingToken),
							Task.Delay(delayInterval, stoppingToken));
					}
				}
				catch (TaskCanceledException)
				{
					logger.LogInformation("The host is being shut down.");
				}
				catch (Exception e)
				{
					logger.LogError(e, $"Final catch in the Meditor trigger for {typeof(T).Name} loop!");
				}
			}
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation($"Started service to trigger {typeof(T).Name} mediator notifications every {delayInterval.TotalSeconds} seconds.");
			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation($"Stopped service to trigger {typeof(T).Name} mediator notifications.");
			await base.StopAsync(cancellationToken);
		}

		private void LogThatYoureStillAround()
		{
			var now = DateTime.UtcNow;
			if (now - lastTimeILogged <= TimeSpan.FromMinutes(1))
			{
				return;
			}

			lastTimeILogged = now;
			logger.LogDebug($"service loop for {typeof(T).Name} mediator notifications doing background work.");
		}
	}
}