using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildDataPrefab : MonoBehaviour
{
    string PlayerTitleID;
    public string userGroupID;
    private AcceptGroupInvitationRequest lastAcceptedInvite;
    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    // Start is called before the first frame update
    void Start()
    {
        //GetGroupId();
        GetUserAccountInfo();
    }


    public void GetUserAccountInfo()
    {
        var request = new GetAccountInfoRequest
        {
        };

        PlayFabClientAPI.GetAccountInfo(request, result => { PlayerTitleID = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id; }, Errorresult => { Debug.Log(Errorresult); });
    }


    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId, string type)
    {
        return new PlayFab.GroupsModels.EntityKey { Id = entityId, Type = type };
    }

    public void GetGroupId()
    {
        var request = new ListMembershipRequest
        {
            Entity = EntityKeyMaker(PlayerTitleID, "title_player_account")
        };
        PlayFabGroupsAPI.ListMembership(request, OnListGroupsForPlayer, OnSharedError);
    }

    private void OnListGroupsForPlayer(ListMembershipResponse response)
    {
        // Check if the player is a member of any group
        if (response.Groups != null && response.Groups.Count > 0)
        {
            // Assuming you want to get the ID of the first group (if there are multiple)
            userGroupID = response.Groups[0].Group.Id;
            Invoke("SimulateAcceptInvite", 0.5f);
            //Debug.Log(response.Groups);
            // Handle the case where the player is not a member of any group
        }
        else
        {
            Debug.Log(response.Groups[0]);
            Debug.Log("ur mother");
            // Handle the case where the player is not a member of any group
        }
    }

    //public void ListMembers(PlayFab.GroupsModels.EntityKey groupId)
    //{
    //    var request = new ListGroupMembersRequest
    //    {
    //        Group = groupId
    //    };
    //    PlayFabGroupsAPI.ListGroupMembers(request, OnListMembers, OnSharedError);
    //}

    //private void OnListMembers(ListGroupMembersResponse response)
    //{
    //    var prevRequest = (ListGroupMembersRequest)response.Request;
    //    foreach (var pair in response.Members)
    //    {
    //        string masterID = pair.Members[0].Lineage["master_player_account"].Id;

    //        var request = new GetAccountInfoRequest
    //        {
    //            PlayFabId = masterID
    //        };

    //        PlayFabClientAPI.GetAccountInfo(request, result =>
    //        {
    //            string displayName = result.AccountInfo.TitleInfo.DisplayName;

    //        }, OnSharedError);

    //    }
    //}

    public void SimulateAcceptGroup()
    {
        var request = new AcceptGroupInvitationRequest { Group = new PlayFab.GroupsModels.EntityKey { Id = userGroupID, Type = "group" }, Entity = new PlayFab.GroupsModels.EntityKey { Id = PlayerTitleID, Type = "title_player_account" } };
        PlayFabGroupsAPI.AcceptGroupInvitation(request, result => {
            Destroy(gameObject);
        }, result => Debug.Log(result));
    }
    public void SimulateDeclineGroup()
    {
        var request = new AcceptGroupInvitationRequest { Group = new PlayFab.GroupsModels.EntityKey { Id = userGroupID, Type = "group" }, Entity = new PlayFab.GroupsModels.EntityKey { Id = PlayerTitleID, Type = "title_player_account" } };
        PlayFabGroupsAPI.AcceptGroupInvitation(request, result => {
            KickMember(userGroupID, new PlayFab.GroupsModels.EntityKey { Id = PlayerTitleID, Type = "title_player_account" });  
            Destroy(gameObject);
        }, result => Debug.Log(result));
    }
    public void KickMember(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId, "group"), Members = new List<PlayFab.GroupsModels.EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
    }
    private void OnKickMembers(PlayFab.GroupsModels.EmptyResponse response)
    {
        var prevRequest = (RemoveMembersRequest)response.Request;
        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
        EntityGroupPairs.Remove(new KeyValuePair<string, string>(prevRequest.Members[0].Id, prevRequest.Group.Id));
    }

    private void OnSharedError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}
