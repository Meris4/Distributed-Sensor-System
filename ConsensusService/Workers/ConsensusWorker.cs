using ConsensusService.Services;
using ConsensusService.Data;
using ConsensusService.Models;

namespace ConsensusService.Workers
{
    public class ConsensusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ConsensusWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var detector = scope.ServiceProvider.GetRequiredService<IMaliciousSensorDetector>();
                var consensus = scope.ServiceProvider.GetRequiredService<IConsensusCalculationService>();
                var db = scope.ServiceProvider.GetRequiredService<SensorDbContext>();

                await detector.EvaluateAsync();

                var end = DateTime.UtcNow;
                var start = end.AddMinutes(-1);

                var value = await consensus.CalculateConsensusAsync(start, end);

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

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
