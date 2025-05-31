using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PosBackend.Services
{
    public class PaymentMonitoringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentMonitoringBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly PaymentMonitoringConfiguration _config;

        public PaymentMonitoringBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PaymentMonitoringBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            _config = new PaymentMonitoringConfiguration();
            _configuration.GetSection("PaymentMonitoring").Bind(_config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Monitoring Background Service started");

            if (!_config.EnableProactiveMonitoring)
            {
                _logger.LogInformation("Proactive payment monitoring is disabled");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunMonitoringTasks();
                    
                    // Wait for the configured interval before running again
                    var delay = TimeSpan.FromMinutes(_config.MonitoringIntervalMinutes);
                    _logger.LogDebug("Payment monitoring cycle completed. Next run in {Minutes} minutes", _config.MonitoringIntervalMinutes);
                    
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in payment monitoring background service");
                    
                    // Wait a shorter time before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Payment Monitoring Background Service stopped");
        }

        private async Task RunMonitoringTasks()
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentMonitoringService = scope.ServiceProvider.GetRequiredService<IPaymentMonitoringService>();

            _logger.LogDebug("Starting payment monitoring tasks");

            // Run all monitoring tasks
            var tasks = new[]
            {
                MonitorCardExpirations(paymentMonitoringService),
                MonitorUpcomingPayments(paymentMonitoringService),
                ProcessFailedPaymentRetries(paymentMonitoringService)
            };

            await Task.WhenAll(tasks);

            _logger.LogDebug("All payment monitoring tasks completed");
        }

        private async Task MonitorCardExpirations(IPaymentMonitoringService paymentMonitoringService)
        {
            try
            {
                _logger.LogDebug("Running card expiration monitoring");
                await paymentMonitoringService.MonitorCardExpirationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in card expiration monitoring");
            }
        }

        private async Task MonitorUpcomingPayments(IPaymentMonitoringService paymentMonitoringService)
        {
            try
            {
                _logger.LogDebug("Running upcoming payment monitoring");
                await paymentMonitoringService.MonitorUpcomingPaymentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in upcoming payment monitoring");
            }
        }

        private async Task ProcessFailedPaymentRetries(IPaymentMonitoringService paymentMonitoringService)
        {
            try
            {
                _logger.LogDebug("Running failed payment retry processing");
                await paymentMonitoringService.ProcessFailedPaymentRetriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in failed payment retry processing");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Monitoring Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
