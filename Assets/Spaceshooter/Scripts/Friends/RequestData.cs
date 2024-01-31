using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

public class RequestData : MonoBehaviour
{
    string myPlayFabID;
    public string RequesteeID;

    // Singleton instance
    private static RequestData instance;

    // Public properties
    public string requesterID { get; private set; }
    public string requesteeDisplayName { get; private set; }

    // Private constructor to prevent instantiation
    private RequestData() { }

    // Public method to get the singleton instance
    public static RequestData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("RequestData").AddComponent<RequestData>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    void Start()
    {
        // Initialize properties
        requesterID = "";
        requesteeDisplayName = "";
        GetUserAccountInfo(result =>
        {
            Debug.Log(myPlayFabID); // This will now log the correct value
                                 // Perform any other initialization or logic here after obtaining PlayFab ID
        });
    }

    public void GetUserAccountInfo(System.Action<GetPlayerProfileResult> callback)
    {
        var request = new GetPlayerProfileRequest
        {

        };

        PlayFabClientAPI.GetPlayerProfile(request, result =>
        {
            myPlayFabID = result.PlayerProfile.PlayerId;
            callback(result); // Invoke the callback passing the result
        },
        errorResult =>
        {
            Debug.Log(errorResult);
        });
    }

    //// Method to set account information
    //public void SetAccountInfo(GetAccountInfoResult result)
    //{
    //    requesterID = result.AccountInfo.PlayFabId;
    //    // You might want to add logic to get requestee display name here
    //}


    void GetRequesteeAccountInfo(string RequesteeID)
    {
        GetAccountInfoRequest request = new GetAccountInfoRequest
        {
            PlayFabId = RequesteeID
        };
        PlayFabClientAPI.GetAccountInfo(request, RequesteeSuccess, DisplayPlayFabError);
    }

    void RequesteeSuccess(GetAccountInfoResult result)
    {
        RequesteeID = result.AccountInfo.PlayFabId;

        // Now you have the PlayFabId of the requestee, and you can use it as needed.
        Debug.Log("Requestee PlayFabId: " + RequesteeID);
    }

    public void AcceptFriend()
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "acceptFriendRequest",
            FunctionParameter = new
            {
                senderID = myPlayFabID,
                reciverID = RequesteeID
            }
        };

        GetRequesteeAccountInfo(RequesteeID);
        PlayFabClientAPI.ExecuteCloudScript(request, result => Debug.Log(myPlayFabID + " accepted friend request to " + RequesteeID), result => Debug.Log("Some error in code dahh"));
        Destroy(gameObject);
    }

    public void DeclineFriend()
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "declineFrinedRequest",
            FunctionParameter = new
            {
                senderID = myPlayFabID,
                reciverID = RequesteeID
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, result => Debug.Log("friend request declined"), result => Debug.Log("Some error in code dahh"));
        Destroy(gameObject);
    }

    void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }
}