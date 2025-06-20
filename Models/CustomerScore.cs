namespace AcmeDealership.Models
{
    public class CustomerScore
    {
        public Customer Customer { get; set; } = new Customer();
        public double Score { get; set; }
        public double AgeScore { get; set; }
        public double DistanceScore { get; set; }
        public double AcceptedOffersScore { get; set; }
        public double CanceledOffersScore { get; set; }
        public double ReplyTimeScore { get; set; }
        public bool IsLowDataCustomer { get; set; }
        
        // Score breakdown for transparency
        public string ScoreBreakdown => 
            $"Age: {AgeScore:F2} (10%), Distance: {DistanceScore:F2} (10%), " +
            $"Accepted: {AcceptedOffersScore:F2} (30%), Canceled: {CanceledOffersScore:F2} (30%), " +
            $"Reply Time: {ReplyTimeScore:F2} (20%) = Total: {Score:F2}";
    }
}
