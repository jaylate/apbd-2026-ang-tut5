using System;

namespace LegacyRenewalApp
{
    public class DiscountCalculator : IDiscountCalculator
    {
        private const decimal SilverDiscount = 0.05m;
        private const decimal GoldDiscount = 0.10m;
        private const decimal PlatinumDiscount = 0.15m;
        private const decimal EducationDiscount = 0.20m;

        private const decimal LongTermLoyaltyDiscount = 0.07m;
        private const decimal BasicLoyaltyDiscount = 0.03m;

        private const decimal LargeTeamDiscount = 0.12m;
        private const decimal MediumTeamDiscount = 0.08m;
        private const decimal SmallTeamDiscount = 0.04m;

        public DiscountResult GetDiscountAmountForCustomerSegment(string segment, decimal baseAmount, bool isEducationEligible)
        {
            return segment switch
            {
                "Silver" => new DiscountResult(baseAmount * SilverDiscount, "silver discount; "),
                "Gold" => new DiscountResult(baseAmount * GoldDiscount, "gold discount; "),
                "Platinum" => new DiscountResult(baseAmount * PlatinumDiscount, "platinum discount; "),
                "Education" => isEducationEligible
                    ? new DiscountResult(baseAmount * EducationDiscount, "education discount; ")
                    : new DiscountResult(0, ""),
                _ => new DiscountResult(0, ""),
            };

        }

        public DiscountResult GetDiscountAmountForLoyalty(decimal yearsWithCompany, decimal baseAmount)
        {
            if (yearsWithCompany >= 5)
            {
                return new DiscountResult(baseAmount * LongTermLoyaltyDiscount, "long-term loyalty discount; ");
            }
            else if (yearsWithCompany >= 2)
            {
                return new DiscountResult(baseAmount * BasicLoyaltyDiscount, "basic loyalty discount; ");
            }
            return new DiscountResult(0, "");
        }

        public DiscountResult GetDiscountAmountForSeatCount(int seatCount, decimal baseAmount)
        {
            if (seatCount >= 50)
            {
                return new DiscountResult(baseAmount * LargeTeamDiscount, "large team discount; ");
            }
            else if (seatCount >= 20)
            {
                return new DiscountResult(baseAmount * MediumTeamDiscount, "medium team discount; ");
            }
            else if (seatCount >= 10)
            {
                return new DiscountResult(baseAmount * SmallTeamDiscount, "small team discount; ");
            }
            return new DiscountResult(0, "");
        }
    }
}
