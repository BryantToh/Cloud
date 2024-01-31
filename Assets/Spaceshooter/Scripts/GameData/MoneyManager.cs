using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public TextMeshProUGUI MoneyText;
    [SerializeField] TextMeshProUGUI Msg;
    public int intendedMoney;
    public int BigMoney;
    [SerializeField]
    PFDataMgr playfabData;
    public List<PlayerControl> playerControlList = new List<PlayerControl>();
    [SerializeField]
    TMP_Text FireRateMsg;
    [SerializeField]
    TMP_Text SpeedMsg;
    [SerializeField]
    TMP_Text BarrierMsg;
    public List<TMP_Text> upgradeNames = new List<TMP_Text>();
    public List<TMP_Text> upgradePrices = new List<TMP_Text>();

    // Start is called before the first frame update
    void Start()
    {
        GetVirtualCurrencies();
        playfabData.GetUserData();
        GetPlayerInventory();
        GetItemsInformation();
    }

    private void Update()
    {

    }

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        if (Msg != null)
        {

            Debug.Log(msg);
            Msg.text += msg + '\n';
        }

    }
    void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport());
    }
    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            intendedMoney = r.VirtualCurrency["BM"];
            BigMoney = intendedMoney;
            MoneyText.text = "BigMoney: " + BigMoney;

        }, OnError);
    }

    public void GetItemsInformation()
    {
        GetCatalogItemsRequest request = new GetCatalogItemsRequest
        {
            CatalogVersion = "Items", // Replace with your catalog version
        };

        PlayFabClientAPI.GetCatalogItems(request, result =>
        {
            List<CatalogItem> items = result.Catalog;
            for (int i = 0; i < upgradeNames.Count; i++)
            {
                if (upgradeNames[i] != null && upgradePrices[i] != null)
                {
                    CatalogItem item = items[i];
                    upgradeNames[i].text = item.DisplayName + ": ";
                    upgradePrices[i].text = "$" + item.VirtualCurrencyPrices["BM"].ToString();
                }
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void GetPlayerInventory()
    {
        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
        result =>
        {
            List<ItemInstance> ii = result.Inventory;
            UpdateMsg("Player Inventory");
            foreach (ItemInstance i in ii)
            {
                if (FireRateMsg != null && SpeedMsg != null && BarrierMsg != null)
                {
                    if (i.ItemId == "FireRate")
                    {
                    FireRateMsg.text = "FireRate" + "\nincreased: " + i.RemainingUses + " times";
                    }
                    if (i.ItemId == "Speed")
                    {
                        SpeedMsg.text = "Speed" + "\nincreased: " + i.RemainingUses + " times";
                    }
                    if (i.ItemId == "Barrier")
                    {
                        BarrierMsg.text = "Barriers" + "\nAvailable: " + i.RemainingUses;
                    }
                }
            }
        }, OnError);
    }

    void BuyUpgrade(int requiredMoney, int amountToSubtract, float fireRateChange, string upgradeName, int speedIncrease, int barrier)
    {
        if (intendedMoney >= requiredMoney)
        {
            var buyreq = new PurchaseItemRequest
            {
                CatalogVersion = "Items",
                ItemId = upgradeName,
                VirtualCurrency = "BM",
                Price = amountToSubtract

            };
            PlayFabClientAPI.PurchaseItem(buyreq, result =>
            {
                OnSubtractMoneySuccess(result, upgradeName, speedIncrease, fireRateChange, barrier);
                GetPlayerInventory();
            }, OnError);
        }
        else
        {
            UpdateMsg("Not enough currency to purchase this upgrade");
        }
    }

    public void BuyFireRate()
    {
        BuyUpgrade(300, 300, 0.0025f, "FireRate", 0, 0);
    }

    public void BuyMoveSpeed()
    {
        BuyUpgrade(500, 500, 0, "Speed", 1, 0);
    }

    public void BuyBarrier()
    {
        BuyUpgrade(600, 600, 0f, "Barrier", 0, 1);
    }

    void OnSubtractMoneySuccess(PurchaseItemResult result, string upgradeName, int speedIncrease, float fireRateChange, int barrier)
    {
        UpdateMsg($"Bought item: " + upgradeName);
        GetVirtualCurrencies();

        foreach (PlayerControl playerControl in playerControlList)
        {
            playerControl.speed += speedIncrease;
            playerControl.fireRate -= fireRateChange;
        }

        playfabData.SetUserData();
    }


    public void GainMoney()
    {
        var gainmon = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "BM",
            Amount = intendedMoney
        };
        PlayFabClientAPI.AddUserVirtualCurrency(gainmon, OnGainMoneySuccess, OnError);
    }

    void OnGainMoneySuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log("Money Gained: " + intendedMoney);
        GetVirtualCurrencies();
        playfabData.GetUserData();

    }
}
