namespace HomemadeFood.Api.Constants
{
    public static class PaymentMethods
    {
        public const string CashOnDelivery =
            "CashOnDelivery";

        public const string CardOnDelivery =
            "CardOnDelivery";

        public const string ValidationPattern =
            "^(CashOnDelivery|CardOnDelivery)$";
    }
}