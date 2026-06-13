using ConsensusService.Data;
using ConsensusService.Models;

namespace ConsensusService.Services.Impl
{
    public class ConsensusBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ConsensusBackgroundWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();
                var consensusService = scope.ServiceProvider.GetRequiredService<IConsensusCalculationService>();
                var detector = scope.ServiceProvider.GetRequiredService<IMaliciousSensorDetector>();

                await detector.EvaluateAsync();

                var end = DateTime.UtcNow;
                var start = end.AddMinutes(-1);

                var value = await consensusService.CalculateConsensusAsync(start, end);

                if (value != null)
                {
                    db.ConsensusValues.Add(new ConsensusValue
                    {
                        Id = Guid.NewGuid(),
                        Value = value.Value,
                        PeriodStart = start,
                        PeriodEnd = end,
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}