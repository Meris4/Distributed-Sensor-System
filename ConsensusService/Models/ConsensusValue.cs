namespace ConsensusService.Models

{
    public class ConsensusValue
    {
        public Guid Id { get; set; }

        public double Value { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
