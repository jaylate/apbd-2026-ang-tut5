namespace LegacyRenewalApp
{
    public interface ITaxCalculator
    {
        public decimal GetTaxAmount(string country, decimal total);
    }
}
