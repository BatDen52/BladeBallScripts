using System;
using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using VContainer;

namespace _Project.IAP
{
    public class IAPServiceUnity : IIAPService, IDetailedStoreListener
    {
        private IStoreController _storeController;

        private IPersistentDataService _persistentDataService;

        private PlayerData _playerData;

        private StaticData _staticData;

        [Inject]
        public void Constructor(IObjectResolver container)
        {
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
        }

        public async void Initialize()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            await UnityServices.InitializeAsync();
            SetupBuilder();
        }

        private void SetupBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var item in _staticData.Settings.InApps)
                builder.AddProduct(item.Id.ToString(), item.Type);

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            //CheckNonConsumable(ncItem.Id);
            //CheckSubscription(sItem.Id);
        }

        public void InitiatePurchase(IAPProduct id)
        {
            _storeController.InitiatePurchase(id.ToString());
        }

        public string GetPrice(IAPProduct id)
        {
            ProductMetadata metadata = _storeController.products.WithID(id.ToString()).metadata;
            // return $"{metadata.localizedPrice} {metadata.isoCurrencyCode}";
            return $"{metadata.localizedPriceString}";
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;

            if (Enum.TryParse(product.definition.id, out IAPProduct myProduct) == false)
            {
                Debug.Log("Unknown In App Product Title");
                return PurchaseProcessingResult.Pending;
            }

            switch (myProduct)
            {
                case IAPProduct.coins1000:
                    AddCoins(1000, product.receipt);
                    break;
                case IAPProduct.coins3750:
                    AddCoins(3750, product.receipt);
                    break;
                case IAPProduct.coins8000:
                    AddCoins(8000, product.receipt);
                    break;
                case IAPProduct.coins14500:
                    AddCoins(14500, product.receipt);
                    break;
                case IAPProduct.coins24000:
                    AddCoins(24000, product.receipt);
                    break;
                case IAPProduct.crystals10:
                    AddCrystals(10, product.receipt);
                    break;
                case IAPProduct.crystals40:
                    AddCrystals(40, product.receipt);
                    break;
                case IAPProduct.crystals90:
                    AddCrystals(90, product.receipt);
                    break;
                case IAPProduct.crystals210:
                    AddCrystals(210, product.receipt);
                    break;
                case IAPProduct.crystals500:
                    AddCrystals(500, product.receipt);
                    break;
                default:
                    Debug.Log("Unknown In App Product Title");
                    break;
            }

            return PurchaseProcessingResult.Complete;
        }

        private void AddCoins(int count, string receipt)
        {
            int quantity = 1;

            if (string.IsNullOrWhiteSpace(receipt) == false)
            {
                Data data = JsonUtility.FromJson<Data>(receipt);

                if (data.Store != "fake")
                {
                    Payload payload = JsonUtility.FromJson<Payload>(data.Payload);
                    PayloadData payloadData = JsonUtility.FromJson<PayloadData>(payload.json);
                    quantity = payloadData.quantity;
                }
            }

            for (int i = 0; i < quantity; i++)
            {
                _playerData.Coins.Value += count;
                _persistentDataService.Save();
                Debug.Log($"Purchase: get {count} coins");
            }
        }

        private void AddCrystals(int count, string receipt)
        {
            int quantity = 1;

            if (string.IsNullOrWhiteSpace(receipt) == false)
            {
                Data data = JsonUtility.FromJson<Data>(receipt);

                if (data.Store != "fake")
                {
                    Payload payload = JsonUtility.FromJson<Payload>(data.Payload);
                    PayloadData payloadData = JsonUtility.FromJson<PayloadData>(payload.json);
                    quantity = payloadData.quantity;
                }
            }

            for (int i = 0; i < quantity; i++)
            {
                _playerData.Crystals.Value += count;
                _persistentDataService.Save();
                Debug.Log($"Purchase: get {count} crystals");
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("failed" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("initialize failed" + error + message);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log("purchase failed" + failureReason);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log("purchase failed" + failureDescription);
        }

        private void CheckNonConsumable(string id)
        {
            if (_storeController != null)
            {
                var product = _storeController.products.WithID(id);

                if (product != null)
                {
                    if (product.hasReceipt)
                    {
                    }
                    else
                    {
                    }
                }
            }
        }

        private void CheckSubscription(string id)
        {
            var subProduct = _storeController.products.WithID(id);

            if (subProduct != null)
            {
                try
                {
                    if (subProduct.hasReceipt)
                    {
                        var subManager = new SubscriptionManager(subProduct, null);
                        var info = subManager.getSubscriptionInfo();

                        if (info.isSubscribed() == Result.True)
                        {
                            Debug.Log("We are subscribed");
                        }
                        else
                        {
                            Debug.Log("Un subscribed");
                        }
                    }
                    else
                    {
                        Debug.Log("receipt not found !!");
                    }
                }
                catch (Exception)
                {
                    Debug.Log("It only work for Google store, app store, amazon store, you are using fake store!!");
                }
            }
            else
            {
                Debug.Log("product not found !!");
            }
        }
    }

    [Serializable]
    public class SkuDetails
    {
        public string productId;
        public string type;
        public string title;
        public string name;
        public string iconUrl;
        public string description;
        public string price;
        public long price_amount_micros;
        public string price_currency_code;
        public string skuDetailsToken;
    }

    [Serializable]
    public class PayloadData
    {
        public string orderId;
        public string packageName;
        public string productId;
        public long purchaseTime;
        public int purchaseState;
        public string purchaseToken;
        public int quantity;
        public bool acknowledged;
    }

    [Serializable]
    public class Payload
    {
        public string json;
        public string signature;
        public List<SkuDetails> skuDetails;
        public PayloadData payloadData;
    }

    [Serializable]
    public class Data
    {
        public string Payload;
        public string Store;
        public string TransactionID;
    }
}