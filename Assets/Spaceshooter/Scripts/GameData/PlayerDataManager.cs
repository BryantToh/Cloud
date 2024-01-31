using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

[System.Serializable]
public class PlayerData
{
    public int Exp;
    public int Level;

    public PlayerData(int _exp, int _Level)
    {
        Exp = _exp;
        Level = _Level;
    }
}

public class PlayerDataManager : MonoBehaviour
{
    private PlayerData currentPlayerData;
    [SerializeField] TextMeshProUGUI Msg;
    [SerializeField] GameController gameController;

    void OnError(PlayFabError e)
    {
        // Set errors flag on error
        Msg.text = "Error:" + e.GenerateErrorReport();
    }

    public void SendJsonData()
    {
        PlayerData data = gameController.ReturnClass();

        string stringValueAsJSon = JsonUtility.ToJson(data);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerData", stringValueAsJSon }
            }
        };
        PlayFabClientAPI.UpdateUserData(request, result => Debug.Log("Data sent successful"), OnError);
    }

    public void LoadJson()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJsonGetData, OnError);
    }
    
    public void OnJsonGetData(GetUserDataResult result)
    {
        Debug.Log("Received JSON Data");

        if (result.Data != null && result.Data.ContainsKey("PlayerData"))
        {
            string Jsonvalues = result.Data["PlayerData"].Value;
            currentPlayerData = JsonUtility.FromJson<PlayerData>(Jsonvalues);
        }
    }
}
