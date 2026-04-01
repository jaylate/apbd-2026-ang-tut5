namespace LegacyRenewalApp
{
    public interface IDiscountCalculator
    {
        public CalculationResult GetDiscountAmountForCustomerSegment(string segment, decimal baseAmount, bool isEducationEligible);
        public CalculationResult GetDiscountAmountForLoyalty(decimal yearsWithCompany, decimal baseAmount);
        public CalculationResult GetDiscountAmountForSeatCount(int seatCount, decimal baseAmount);
    }
}
