namespace _Project.IAP
{
    public interface IIAPService
    {
        void Initialize();
        void InitiatePurchase(IAPProduct productId);
        string GetPrice(IAPProduct id);
    }
}