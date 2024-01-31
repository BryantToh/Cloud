using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildData : MonoBehaviour
{
    string PlayerTitleID;
    string userGroupID;
    private AcceptGroupInvitationRequest lastAcceptedInvite;
    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    // Start is called before the first frame update
    void Start()
    {
        //GetGroupId();
        GetUserAccountInfo(result =>
        {
            Debug.Log(PlayerTitleID); // This will now log the correct value
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
            PlayerTitleID = result.PlayerProfile.PlayerId;
            callback(result); // Invoke the callback passing the result
        },
        errorResult =>
        {
            Debug.Log(errorResult);
        });
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

    public void SimulateAcceptInvite()
    {
        // Simulate receiving an invitation
        var simulatedInvite = new AcceptGroupInvitationRequest
        {
            Entity = new PlayFab.GroupsModels.EntityKey { Id = PlayerTitleID, Type = "title_player_account" },
            Group = new PlayFab.GroupsModels.EntityKey { Id = userGroupID, Type = "group" }
        };

        // Store the simulated invite for later processing in OnAcceptInvite
        lastAcceptedInvite = simulatedInvite;

        // Call OnAcceptInvite as if it was triggered by a PlayFab response
        OnAcceptInvite(new PlayFab.GroupsModels.EmptyResponse());
    }

    public void OnAcceptInvite(PlayFab.GroupsModels.EmptyResponse response)
    {
        if (lastAcceptedInvite != null)
        {
            Debug.Log("Entity Added to Group: " + lastAcceptedInvite.Entity.Id + " to " + lastAcceptedInvite.Group.Id);
            EntityGroupPairs.Add(new KeyValuePair<string, string>(lastAcceptedInvite.Entity.Id, lastAcceptedInvite.Group.Id));

            // Optionally, clear the stored invite after processing
            lastAcceptedInvite = null;
        }
        else
        {
            Debug.LogError("No pending invite found.");
        }
    }
    //public void OnAcceptInvite(PlayFab.GroupsModels.EmptyResponse response)
    //{
    //    var prevRequest = (AcceptGroupInvitationRequest)response.Request;
    //    Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
    //    EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, prevRequest.Group.Id));
    //}

    //public void OnAcceptApplication(PlayFab.GroupsModels.EmptyResponse response)
    //{
    //    var prevRequest = (AcceptGroupApplicationRequest)response.Request;
    //    Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
    //}
    //public void Decline()
    //{
    //    SimulateAcceptInvite();
    //    KickMember(userGroupID);
    //}

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
