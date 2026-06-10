namespace ConsensusService.Services
{
    public interface IConsensusCalculationService
    {
        Task<double?> CalculateConsensusAsync(DateTime start, DateTime end);
    }
}
