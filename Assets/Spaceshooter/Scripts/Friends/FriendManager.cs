using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class FriendManager : MonoBehaviour
{
    [SerializeField] GameObject FriendPrefab, PendingPrefab, RequestPrefab, displayList;
    [SerializeField] TextMeshProUGUI leaderboarddisplay;
    [SerializeField] TMP_InputField tgtFriend, tgtunfrnd;
    List<FriendInfo> _friends = null;
    enum FriendIdType { PlayFabId, Username, Email, DisplayName };

    private string myPlayFabID;

    //RequestData requestData = RequestData.Instance;

    private void Start()
    {
        GetAccountInfo();
    }

    void GetAccountInfo()
    {
        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, Success, DisplayPlayFabError);
    }

    void Success(GetAccountInfoResult result)
    {
        myPlayFabID = result.AccountInfo.PlayFabId;
    }

    //Display Friend Code
    void DisplayFriends(List<FriendInfo> friendsCache, int listType)
    {
        for (int i = displayList.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = displayList.transform.GetChild(i);
            Destroy(child.gameObject);
        }
 

        friendsCache.ForEach(f => {

            switch (listType)
            {
                case 0:
                    if (f.Tags.Contains("friend"))
                    {
                        Debug.Log("Friends");
                        GameObject friendPrefab = Instantiate(FriendPrefab);
                        friendPrefab.transform.SetParent(displayList.transform);
                        //setting the Name placeholder to name of friend
                        friendPrefab.transform.Find("Name").GetComponent<TMP_Text>().text = f.TitleDisplayName;
                    }
                    else return;
                    break;
                case 1:
                    if (f.Tags.Contains("requestee"))
                    {
                        GameObject pendingPrefab = Instantiate(PendingPrefab);
                        pendingPrefab.transform.SetParent(displayList.transform);
                        pendingPrefab.transform.Find("Name").GetComponent<TMP_Text>().text = f.TitleDisplayName;
                        pendingPrefab.GetComponent<RequestData>().RequesteeID = f.FriendPlayFabId;
                    }
                    else return;

                    break;
                case 2:
                    if (f.Tags.Contains("requester"))
                    {
                        GameObject requestPrefab = Instantiate(RequestPrefab);
                        requestPrefab.transform.SetParent(displayList.transform);
                        //setting the Name placeholder to name of frined
                        requestPrefab.transform.Find("Name").GetComponent<TMP_Text>().text = f.TitleDisplayName;
                        requestPrefab.GetComponent<RequestData>().RequesteeID = f.FriendPlayFabId;
                    }
                    else
                        return;
                    break;
            }
        });
    }

    void DisplayLeaderboard(List<FriendInfo> friendsCache)
    {
        leaderboarddisplay.text = "";
        // Fetch the leaderboard once
        PlayFabClientAPI.GetFriendLeaderboard(
            new GetFriendLeaderboardRequest { StatisticName = "Highscore", MaxResultsCount = 10 },
            result =>
            {
                // Sort the leaderboard by score in descending order
                var sortedLeaderboard = result.Leaderboard.OrderByDescending(entry => entry.StatValue).ToList();

                // Create a dictionary to map PlayFabId to leaderboard entry
                var leaderboardMap = sortedLeaderboard.ToDictionary(entry => entry.PlayFabId);

                // Create a sorted friendsCache list based on the order in the friendsCache list
                var sortedFriendsCache = friendsCache
                        .Where(friend => friend.Tags.Contains("friend"))
                        .OrderByDescending(friend => leaderboardMap.ContainsKey(friend.FriendPlayFabId) ? leaderboardMap[friend.FriendPlayFabId].StatValue : 0)
                        .ToList();

                // Iterate over the sorted friendsCache and display information
                for (int i = 0; i < sortedFriendsCache.Count; i++)
                {
                    var friend = sortedFriendsCache[i];

                    if (leaderboardMap.TryGetValue(friend.FriendPlayFabId, out var leaderboardEntry))
                    {
                        // Display information for the friend
                        string onerow = (i + 1) + ". " + leaderboardEntry.DisplayName + " | " + leaderboardEntry.StatValue + "\n";
                        leaderboarddisplay.text += onerow;
                        Debug.Log(onerow);
                    }
                }
            },
            DisplayPlayFabError
        );
    }



    public void GetLeaderboard()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result =>
        {
            _friends = result.Friends;
            DisplayLeaderboard(_friends); // triggers your UI
        }, DisplayPlayFabError);
    }


    //Get Friend Code
    public void GetFriends(int listType)
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends, listType); // triggers your UI
        }, DisplayPlayFabError);
    }

    //Add Friend Code
    void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
        }, DisplayPlayFabError);
    }
    public void OnAddFriend()
    {
        //to add friend based on display name
        //AddFriend(FriendIdType.DisplayName, tgtFriend.text);
        //getting friend's playfab ID
        string friendPlayID = null;
        var requestFriendID = new GetAccountInfoRequest { TitleDisplayName = tgtFriend.text };
        PlayFabClientAPI.GetAccountInfo(requestFriendID, result => {
            friendPlayID = result.AccountInfo.PlayFabId;
            Debug.Log("friend's id is: " + result.AccountInfo.PlayFabId);

            if (myPlayFabID != null && friendPlayID != null)
            {
                sendFriendRequest(myPlayFabID, friendPlayID);
            }
            else
            {
                Debug.Log("Can't get own ID dah");
            }
        }, result => Debug.Log("error somewhere dah"));

    }
    //runs the cloud script
    void sendFriendRequest(string myPlayFabID, string friendPlayID)
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "sendFriendRequest",
            FunctionParameter = new
            {
                senderID = myPlayFabID,
                reciverID = friendPlayID
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, result => Debug.Log(myPlayFabID + " sent friend request to " + friendPlayID), result => Debug.Log("Some error in code dahh"));
    }


    //Remove Friend Code
    void RemoveFriend(FriendInfo friendInfo)
    { //to investigat
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result => {
            _friends.Remove(friendInfo);
        }, DisplayPlayFabError);
    }

    //Friend Leaderboard Code
    public void OnGetFriendLB()
    {
        PlayFabClientAPI.GetFriendLeaderboard(
        new GetFriendLeaderboardRequest { StatisticName = "highscore", MaxResultsCount = 10 },
        r => {
            leaderboarddisplay.text = "Friends LB\n";
            foreach (var item in r.Leaderboard)
            {
                string onerow = item.Position + "/" + item.DisplayName + "/" + item.StatValue + "\n";
                Debug.Log(onerow);
                leaderboarddisplay.text += onerow;
            }
        }, DisplayPlayFabError);
    }

    public void OnUnFriend()
    {
        RemoveFriend(tgtunfrnd.text);
    }
    void RemoveFriend(string pfid)
    {
        var req = new RemoveFriendRequest
        {
            FriendPlayFabId = pfid
        };
        PlayFabClientAPI.RemoveFriend(req
        , result => {
            Debug.Log("unfriend!");
        }, DisplayPlayFabError);
    }

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }
    void DisplayError(string error) { Debug.LogError(error); }
}
