namespace LegacyRenewalApp
{
    public interface IDiscountCalculator
    {
        public DiscountResult GetDiscountAmountForCustomerSegment(string segment, decimal baseAmount, bool isEducationEligible);
        public DiscountResult GetDiscountAmountForLoyalty(decimal yearsWithCompany, decimal baseAmount);
        public DiscountResult GetDiscountAmountForSeatCount(int seatCount, decimal baseAmount);
    }
}
