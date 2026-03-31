using System;

namespace LegacyRenewalApp
{
    public class SubscriptionPlan
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyPricePerSeat { get; set; }
        public decimal SetupFee { get; set; }
        public bool IsEducationEligible { get; set; }

        public decimal GetSupportFee()
        {
            return Code switch
            {
                "START" => 250m,
                "PRO" => 400m,
                "ENTERPRISE" => 700m,
                _ => throw new ArgumentException("Unsupported plan code"),
            };
        }
    }
}
