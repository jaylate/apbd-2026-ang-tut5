namespace LegacyRenewalApp
{
    public class TaxCalculator : ITaxCalculator
    {
        public const decimal PolandTaxRate = 0.23m;
        public const decimal GermanyTaxRate = 0.19m;
        public const decimal CzechRepublicTaxRate = 0.21m;
        public const decimal NorwayTaxRate = 0.25m;
        public const decimal DefaultTaxRate = 0.20m;

        public decimal GetTaxAmount(string country, decimal total)
        {
            return total * country switch
            {
                "Poland" => PolandTaxRate,
                "Germany" => GermanyTaxRate,
                "Czech Republic" => CzechRepublicTaxRate,
                "Norway" => NorwayTaxRate,
                _ => DefaultTaxRate,
            };
        }
    }
}
