using System;

namespace LegacyRenewalApp
{
    public class RenewalInvoice
    {
        public int CustomerId { get; init; }
        public string PlanCode { get; init; }
        public DateTime GeneratedAt { get; init; }
        public string InvoiceNumber { get; init; }

        public RenewalInvoice(int customerId, string planCode, DateTime generatedAt)
        {
            CustomerId = customerId;
            PlanCode = planCode;
            GeneratedAt = generatedAt;
            InvoiceNumber = $"INV-{generatedAt:yyyyMMdd}-{customerId}-{planCode}";
        }

        public string CustomerName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public int SeatCount { get; set; }

        private decimal _BaseAmount;
        public decimal BaseAmount
        {
            get { return _BaseAmount; }
            set { _BaseAmount = round(value); }
        }
        private decimal _DiscountAmount;
        public decimal DiscountAmount
        {
            get { return _DiscountAmount; }
            set { _DiscountAmount = round(value); }
        }
        private decimal _SupportFee;
        public decimal SupportFee
        {
            get { return _SupportFee; }
            set { _SupportFee = round(value); }
        }
        private decimal _PaymentFee;
        public decimal PaymentFee
        {
            get { return _PaymentFee; }
            set { _PaymentFee = round(value); }
        }
        private decimal _TaxAmount;
        public decimal TaxAmount
        {
            get { return _TaxAmount; }
            set { _TaxAmount = round(value); }
        }
        private decimal _FinalAmount;
        public decimal FinalAmount
        {
            get { return _FinalAmount; }
            set { _FinalAmount = round(value); }
        }
        public string Notes { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"InvoiceNumber={InvoiceNumber}, Customer={CustomerName}, Plan={PlanCode}, Seats={SeatCount}, FinalAmount={FinalAmount:F2}, Notes={Notes}";
        }

        private decimal round(decimal number)
        {
            return Math.Round(number, 2, MidpointRounding.AwayFromZero);
        }
    }
}
