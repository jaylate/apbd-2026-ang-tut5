using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingGateway _billingGateway;
        private readonly IRenewalValidator _renewalValidator;
        private readonly IDiscountCalculator _discountCalculator;
        private readonly IPaymentFeeCalculator _paymentFeeCalculator;
        private readonly ITaxCalculator _taxCalculator;
        private readonly INoteBuilder _noteBuilder;

        public SubscriptionRenewalService() : this(
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new BillingGatewayAdapter(),
            new RenewalValidator(),
            new DiscountCalculator(),
            new PaymentFeeCalculator(),
            new TaxCalculator(),
        new NoteBuilder())
        { }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IBillingGateway billingGateway,
            IRenewalValidator renewalValidator,
            IDiscountCalculator discountCalculator,
            IPaymentFeeCalculator paymentFeeCalculator,
            ITaxCalculator taxCalculator,
        INoteBuilder noteBuilder)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingGateway = billingGateway;
            _renewalValidator = renewalValidator;
            _discountCalculator = discountCalculator;
            _paymentFeeCalculator = paymentFeeCalculator;
            _taxCalculator = taxCalculator;
            _noteBuilder = noteBuilder;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _renewalValidator.Validate(customerId, planCode, seatCount, paymentMethod);

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

            var segmentDiscount = _discountCalculator.GetDiscountAmountForCustomerSegment(
                customer.Segment,
                baseAmount,
                plan.IsEducationEligible
            );
            discountAmount += segmentDiscount.Amount;
            _noteBuilder.AddNote(segmentDiscount.Description);

            var yearsDiscount = _discountCalculator.GetDiscountAmountForLoyalty(
                customer.YearsWithCompany,
                baseAmount
            );
            discountAmount += yearsDiscount.Amount;
            _noteBuilder.AddNote(yearsDiscount.Description);

            var seatDiscount = _discountCalculator.GetDiscountAmountForSeatCount(
                seatCount,
                baseAmount
            );
            discountAmount += seatDiscount.Amount;
            _noteBuilder.AddNote(seatDiscount.Description);

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                _noteBuilder.AddNote($"loyalty points used: {pointsToUse}");
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                _noteBuilder.AddNote("minimum discounted subtotal applied");
            }

            decimal supportFee = 0m;
            if (includePremiumSupport)
            {
                supportFee = plan.GetSupportFee();

                _noteBuilder.AddNote("premium support included");
            }

            var paymentFeeResult = _paymentFeeCalculator.GetPaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            decimal paymentFee = paymentFeeResult.Amount;
            _noteBuilder.AddNote(paymentFeeResult.Description);

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = _taxCalculator.GetTaxAmount(customer.Country, taxBase);
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                _noteBuilder.AddNote("minimum invoice amount applied");
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
                Notes = _noteBuilder.ToString(),
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
