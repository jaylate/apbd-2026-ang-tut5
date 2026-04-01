using System;

namespace LegacyRenewalApp
{
    public class PaymentFeeCalculator : IPaymentFeeCalculator
    {
        public const decimal CardPaymentFee = 0.02m;
        public const decimal BankTransferFee = 0.01m;
        public const decimal PaypalFee = 0.035m;

        public CalculationResult GetPaymentFee(string paymentMethod, decimal total)
        {
            return paymentMethod switch
            {
                "CARD" => new CalculationResult(total * CardPaymentFee, "card payment fee"),
                "BANK_TRANSFER" => new CalculationResult(total * BankTransferFee, "bank transfer fee"),
                "PAYPAL" => new CalculationResult(total * PaypalFee, "paypal fee"),
                "INVOICE" => new CalculationResult(0m, "invoice payment"),
                _ => throw new ArgumentException("Unsupported payment method")
            };
        }
    }
}
