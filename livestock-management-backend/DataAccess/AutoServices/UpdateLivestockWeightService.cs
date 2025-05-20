using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataAccess.AutoServices
{
    public class UpdateLivestockWeightService : BackgroundService
    {
        private readonly ILogger<UpdateLivestockWeightService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateLivestockWeightService(ILogger<UpdateLivestockWeightService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var initialDelay = nextMidnight - now;

            //wait to 00:00:00
            _logger.LogInformation($"[{this.GetType().Name}] delays in {initialDelay}");
            await Task.Delay(initialDelay, stoppingToken);

            //run daily at 00:00:00
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"[{this.GetType().Name}] starts updating livestock weights");

                    //update weight for livestocks of status KHỎE_MẠNH
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<LmsContext>();

                        var livestocksToUpdate = await context.Livestocks
                            .Where(o => o.Status == BusinessObjects.Constants.LmsConstants.livestock_status.KHỎE_MẠNH)
                            .ToArrayAsync();
                        if (livestocksToUpdate == null || !livestocksToUpdate.Any())
                        {
                            _logger.LogWarning($"[{this.GetType().Name}] no livestocks of status KHỎE_MẠNH found");
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                            continue;
                        }
                        var dicSpecies = await context.Species
                            .ToDictionaryAsync(o => o.Id, o => o.GrowthRate ?? 0);
                        if (dicSpecies == null || !dicSpecies.Any())
                            throw new Exception("No species found");
                        foreach (var livestock in livestocksToUpdate)
                        {
                            if (string.IsNullOrEmpty(livestock.SpeciesId))
                            {
                                _logger.LogWarning($"[{this.GetType().Name}] livestock {livestock.Id} missing specie id");
                                continue;
                            }
                            decimal weightOrigin = livestock.WeightOrigin ?? 0;
                            decimal growthRate = dicSpecies.TryGetValue(livestock.SpeciesId, out var rate) ? rate : 0;
                            livestock.WeightEstimate = weightOrigin + weightOrigin * growthRate / 100;
                            livestock.UpdatedAt = DateTime.Now;
                        }
                        await context.SaveChangesAsync();
                    }

                    _logger.LogInformation($"[{this.GetType().Name}] finishes updating livestock weights");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{this.GetType().Name}] error: {ex.Message}");
                }

                //wait to 00:00:00 the next day
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
