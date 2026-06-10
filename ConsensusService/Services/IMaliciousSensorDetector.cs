namespace ConsensusService.Services
{
    public interface IMaliciousSensorDetector
    {
        Task EvaluateAsync();
    }
}
