namespace LegacyRenewalApp
{
    public interface IRenewalValidator
    {
        public void Validate(int customerId, string planCode, int seatCount, string paymentMethod);
    }
}
