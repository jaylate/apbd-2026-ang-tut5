namespace LegacyRenewalApp
{
    public interface IBillingGateway
    {
        public void SaveInvoice(RenewalInvoice invoice);
        public void SendEmail(string email, string subject, string body);
    }
}
