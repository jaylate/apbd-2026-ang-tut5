using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {

        public ICustomerRepository CustomerRepository { get; set; } = new CustomerRepository();
        public ISubscriptionPlanRepository PlanRepository { get; set; } = new SubscriptionPlanRepository();
        public IBillingGateway BillingGateway { get; set; } = new BillingGatewayAdapter();

        public IRenewalValidator RenewalValidator { get; set; } = new RenewalValidator();
        public IDiscountCalculator DiscountCalculator { get; set; } = new DiscountCalculator();
        public IPaymentFeeCalculator PaymentFeeCalculator { get; set; } = new PaymentFeeCalculator();

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            RenewalValidator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = CustomerRepository.GetById(customerId);
            var plan = PlanRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            var segmentDiscount = DiscountCalculator.GetDiscountAmountForCustomerSegment(
                customer.Segment,
                baseAmount,
                plan.IsEducationEligible
            );
            discountAmount += segmentDiscount.Amount;
            notes += segmentDiscount.Description;

            var yearsDiscount = DiscountCalculator.GetDiscountAmountForLoyalty(
            customer.YearsWithCompany,
            baseAmount
            );
            discountAmount += yearsDiscount.Amount;
            notes += yearsDiscount.Description;

            var seatDiscount = DiscountCalculator.GetDiscountAmountForSeatCount(
            seatCount,
            baseAmount
            );
            discountAmount += seatDiscount.Amount;
            notes += seatDiscount.Description;

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            decimal supportFee = 0m;
            if (includePremiumSupport)
            {
                supportFee = plan.GetSupportFee();

                notes += "premium support included; ";
            }

            var paymentFeeResult = PaymentFeeCalculator.GetPaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            decimal paymentFee = paymentFeeResult.Amount;
            notes += paymentFeeResult.Description;

            decimal taxRate = customer.Country switch
            {
                "Poland" => 0.23m,
                "Germany" => 0.19m,
                "Czech Republic" => 0.21m,
                "Norway" => 0.25m,
                _ => 0.20m,
            };

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = new RenewalInvoice(customerId, normalizedPlanCode, DateTime.UtcNow)
            {
                CustomerName = customer.FullName,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = baseAmount,
                DiscountAmount = discountAmount,
                SupportFee = supportFee,
                PaymentFee = paymentFee,
                TaxAmount = taxAmount,
                FinalAmount = finalAmount,
                Notes = notes.Trim(),
            };

            BillingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                BillingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
