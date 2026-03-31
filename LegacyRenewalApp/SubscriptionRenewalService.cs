using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {

        CustomerRepository _customerRepository = new CustomerRepository();
        SubscriptionPlanRepository _planRepository = new SubscriptionPlanRepository();

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            switch (customer.Segment)
            {
                case "Silver":
                    discountAmount += baseAmount * 0.05m;
                    notes += "silver discount; ";
                    break;
                case "Gold":
                    discountAmount += baseAmount * 0.10m;
                    notes += "gold discount; ";
                    break;
                case "Platinum":
                    discountAmount += baseAmount * 0.15m;
                    notes += "platinum discount; ";
                    break;
                case "Education":
                    if (plan.IsEducationEligible)
                    {
                        discountAmount += baseAmount * 0.20m;
                        notes += "education discount; ";
                    }
                    break;
            }

            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes += "long-term loyalty discount; ";
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes += "basic loyalty discount; ";
            }

            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes += "large team discount; ";
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes += "medium team discount; ";
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes += "small team discount; ";
            }

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
                switch (normalizedPlanCode)
                {
                    case "START":
                        supportFee = 250m;
                        break;
                    case "PRO":
                        supportFee = 400m;
                        break;
                    case "ENTERPRISE":
                        supportFee = 700m;
                        break;
                }

                notes += "premium support included; ";
            }

            decimal paymentFee = 0m;
            switch (normalizedPaymentMethod)
            {
                case "CARD":
                    paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
                    notes += "card payment fee; ";
                    break;
                case "BANK_TRANSFER":
                    paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
                    notes += "bank transfer fee; ";
                    break;
                case "PAYPAL":
                    paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
                    notes += "paypal fee; ";
                    break;
                case "INVOICE":
                    paymentFee = 0m;
                    notes += "invoice payment; ";
                    break;
                default:
                    throw new ArgumentException("Unsupported payment method");
            }

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

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = baseAmount,
                DiscountAmount = discountAmount,
                SupportFee = supportFee,
                PaymentFee = paymentFee,
                TaxAmount = taxAmount,
                FinalAmount = finalAmount,
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            LegacyBillingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                LegacyBillingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
