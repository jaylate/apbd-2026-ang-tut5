namespace LegacyRenewalApp
{
    public interface IPaymentFeeCalculator
    {
        public CalculationResult GetPaymentFee(string paymentMethod, decimal total);
    }
}
