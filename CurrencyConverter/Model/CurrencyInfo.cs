namespace CurrencyConverter.Model
{
    public class CurrencyInfo
    {
        public decimal CurrentRate { get; set; }
        public decimal RateOnDate { get; set; }
        public decimal Delta => decimal.Round(RateOnDate - CurrentRate, 4);
        public decimal CurrentRateToDollar { get; set; }
        public decimal RateOnDateToDollar { get; set; }
        public decimal DeltaToDollar { get; set; }
    }
}
