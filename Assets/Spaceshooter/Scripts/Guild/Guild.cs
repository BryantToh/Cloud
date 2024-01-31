using PlayFab;
using PlayFab.GroupsModels;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;

[Serializable]
public class Guild : MonoBehaviour
{
    string PlayerTitleID;
    string userGroupID;
    [SerializeField] TMP_InputField GuideName;
    [SerializeField] GameObject createOwnGuild, invUsers;
    [SerializeField] GameObject invitedUser, displayList, showInvites;
    [SerializeField] TMP_Text txtDisplay;
    [SerializeField] TMP_InputField GuildDescription;
    [SerializeField] TMP_InputField inviteplayerID;
    [SerializeField] GameObject GuildReq;
    PlayFab.GroupsModels.EntityKey ghostMember = EntityKeyMaker("96FC7B46D20135CE", "title_player_account");

    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    [System.Serializable]
    public class GuildData
    {
        public List<GuildInfo> Groups;
    }

    [System.Serializable]
    public class GuildInfo
    {
        public string GroupName;
    }

    private void Start()
    {
        GetUserAccountInfo();
        if (createOwnGuild != null)
            createOwnGuild.SetActive(false);

        if (invUsers != null)
            invUsers.SetActive(false);

        GuildReq.SetActive(false);
    }

    public void GetUserAccountInfo()
    {
        var request = new GetAccountInfoRequest
        {
        };
        PlayFabClientAPI.GetAccountInfo(request, result => { PlayerTitleID = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id; GetGroupId(); }, Errorresult => { Debug.Log(Errorresult); });
    }

    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId, string type)
    {
        return new PlayFab.GroupsModels.EntityKey { Id = entityId, Type = type };
    }
    private void OnSharedError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    //private void GetGroup(string groupName)
    //{
    //    var request = new GetGroupRequest
    //    {
    //        GroupName = groupName,
    //    };

    //    // Now you can proceed with making the request and handling the response
    //    // For example, you might want to call a method to handle the response:
    //    PlayFabGroupsAPI.GetGroup(request, HandleGroupRequest, OnSharedError);
    //}

    //private void HandleGroupRequest(GetGroupResponse response)
    //{
    //    ////// Your logic to handle the group request response goes here
    //    ////// You can process the response, display it, or perform any other actions based on your requirements
    //    ////foreach (var groupnames in response.GroupName)
    //    ////{
    //    ////}
    //    ////EntityGroupPairs.Add(new KeyValuePair<string, string>(response.Group.Id, response.GroupName));

    //    var request = new ListGroupMembersRequest
    //    {
    //        //Group = EntityKeyMaker(GetGroup("salagau"), "title_player_account") 
    //        Group = response.Group,
    //    };
    //    PlayFabGroupsAPI.ListGroupMembers(request, OnListMembers, OnSharedError);

    //}

    //public void ListMembers()
    //{
    //    //string groupname = 
    //    var request = new ListGroupMembersRequest
    //    {
    //        //Group = EntityKeyMaker(GetGroup("salagau"), "title_player_account") 
    //        Group =
    //    };
    //    PlayFabGroupsAPI.ListGroupMembers(request, OnListMembers, OnSharedError);
    //}

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
            // For simplicity, let's assume the player is a member of the first group in the response
            userGroupID = response.Groups[0].Group.Id;
            OnShowGuildMembers();
        }
        else
        {
            Debug.Log("Player is not a member of any group.");
            // Handle the case where the player is not a member of any group
        }
    }

    public void OnListAllGuild()
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "ListMembership",
            FunctionParameter = new
            {
                entityId = "96FC7B46D20135CE"
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            // Deserialize the JSON string into a GuildData object
            GuildData guildData = JsonUtility.FromJson<GuildData>(result.FunctionResult.ToString());

            if (guildData != null && guildData.Groups != null)
            {
                string guildNames = "";

                foreach (var guild in guildData.Groups)
                {
                    Debug.Log($"{guild.GroupName}");
                    guildNames += $"{guild.GroupName}\n";
                }

                txtDisplay.text = guildNames;
            }
            else
            {
                Debug.Log("Invalid or missing data in the JSON response.");
            }

        }, result=>Debug.Log(result));
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
    //    if (txtDisplay != null)
    //    {
    //        txtDisplay.text = "";
    //        var prevRequest = (ListGroupMembersRequest)response.Request;
    //        foreach (var pair in response.Members)
    //        {
    //            string masterID = pair.Members[0].Lineage["master_player_account"].Id;

    //            var request = new GetAccountInfoRequest
    //            {
    //                PlayFabId = masterID
    //            };

    //            PlayFabClientAPI.GetAccountInfo(request, result =>
    //            {
    //                string displayName = result.AccountInfo.TitleInfo.DisplayName;
    //                txtDisplay.text += displayName + "\n";

    //            }, OnSharedError);

    //            txtDisplay.text += pair.RoleId + "\n";
    //        }
    //    }
    //}


    private void OnShowGuildMembers()
    {
        txtDisplay.text = "";

        PlayFabGroupsAPI.ListGroupMembers(new ListGroupMembersRequest
        {
            Group = EntityKeyMaker(userGroupID, "group")
        },
            result =>
            {
                foreach (var item in result.Members)
                {
                    foreach (var member in item.Members)
                    {
                        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
                        {
                            PlayFabId = member.Lineage["master_player_account"].Id
                        }, pResult =>
                        {
                            txtDisplay.text += pResult.PlayerProfile.DisplayName + "\n";
                        }, OnSharedError);
                    }
                }
            },
            error => Debug.LogError($"Error listing group membership: {error.GenerateErrorReport()}")
            );
    }

    //public void ListGroups(PlayFab.GroupsModels.EntityKey entityKey)
    //{
    //    var request = new ListMembershipRequest { Entity = entityKey };
    //    PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
    //}

    //private void OnListGroups(ListMembershipResponse response)
    //{
    //    var prevRequest = (ListMembershipRequest)response.Request;
    //    foreach (var pair in response.Groups)
    //    {
    //        GroupNameById[pair.Group.Id] = pair.GroupName;
    //        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));

    //        var getRequest = new GetObjectsRequest { Entity = new PlayFab.DataModels.EntityKey { Id = pair.Group.Id, Type = "group" } };
    //        PlayFabDataAPI.GetObjects(getRequest,
    //             result =>
    //             {
    //                 Debug.Log("Objects retrieved successfully:");
    //                 foreach (var objectData in result.Objects)
    //                 {
    //                     if (objectData.Key == "Description")
    //                     {
    //                         Debug.Log(objectData.Value.DataObject.ToString());
    //                     }

    //                 }
    //             },
    //            result => Debug.Log(result));
    //    }
    //}

    public void CreateGroup()
    {
        // A player-controlled entity creates a new group
        var request = new CreateGroupRequest { GroupName = GuideName.text, Entity = EntityKeyMaker(PlayerTitleID, "title_player_account") };
        PlayFabGroupsAPI.CreateGroup(request, OnCreateGroup, OnSharedError);
    }

    private void OnCreateGroup(CreateGroupResponse response)
    {
        Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);

        CreateDescription(GuildDescription.text, response.Group.Id);
        createOwnGuild.SetActive(false);

        var prevRequest = (CreateGroupRequest)response.Request;
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, response.Group.Id));
        GroupNameById[response.Group.Id] = response.GroupName;

        //ghost member
        var request = new ApplyToGroupRequest { Group = EntityKeyMaker(response.Group.Id, "group"), Entity = ghostMember };
        PlayFabGroupsAPI.ApplyToGroup(request, result => {
            var prevRequest = (ApplyToGroupRequest)result.Request;
            // where the ghost member 'accepts' the guild
            var request = new AcceptGroupApplicationRequest { Group = prevRequest.Group, Entity = ghostMember };
            PlayFabGroupsAPI.AcceptGroupApplication(request, result => {
                var prevRequest = (AcceptGroupApplicationRequest)result.Request;
                Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);

                //Make a ghost role
                var AddGhostRole = new CreateGroupRoleRequest { Group = EntityKeyMaker(response.Group.Id, "group"), RoleId = "ghost", RoleName = "Fodder" };
                PlayFabGroupsAPI.CreateRole(AddGhostRole, result => {
                    //make ghost member role to be ghost
                    var MembersToChange = new List<PlayFab.GroupsModels.EntityKey>();
                    MembersToChange.Add(ghostMember);
                    var changeRole = new ChangeMemberRoleRequest { Group = EntityKeyMaker(response.Group.Id, "group"), Members = MembersToChange, OriginRoleId = "members", DestinationRoleId = "ghost" };
                    PlayFabGroupsAPI.ChangeMemberRole(changeRole, result => Debug.Log("Changed member to AI"), result => Debug.Log(result));
                }, result => Debug.Log(result));
            }, OnSharedError);
        }, OnSharedError);
    }

    public void CreateDescription(string description, string GroupID)
    {
        if (description == "")
        {
            description = "This guild has no description";
        }
        var dataList = new List<SetObject>()
        {
            new SetObject()
            {
                ObjectName = "Description",
                //input the description
                DataObject = description
            }
        };
        PlayFabDataAPI.SetObjects(new SetObjectsRequest()
        {
            Entity = new PlayFab.DataModels.EntityKey { Id = GroupID, Type = "group" }, // Saved from GetEntityToken, or a specified key created from a titlePlayerId, CharacterId, etc
            Objects = dataList,
        }, (setResult) =>
        {
            Debug.Log(setResult.ProfileVersion);
        }, result => Debug.Log(result));

        var request = new ListGroupApplicationsRequest
        {
            Group = new PlayFab.GroupsModels.EntityKey { Id = GroupID, Type = "group" }
        };
        PlayFabGroupsAPI.ListGroupApplications(request,
            result =>
            {
                foreach (var application in result.Applications)
                {
                    Debug.Log($"Pending Request: GroupID - {application.Group.Id}, PlayerID - {application.Entity.Key.Id}");
                }
            },
            error => Debug.LogError($"Error listing group applications: {error.ErrorMessage}")
        );
    }

    //public void DeleteGroup()
    //{
    //    // Get the value from the input field
    //    string inputValue = deleteGrp.text;

    //    // Pass the input value to the EntityKeyMaker method
    //    var request = new DeleteGroupRequest { Group = EntityKeyMaker(inputValue, "group") };
    //    PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
    //}

    //private void OnDeleteGroup(PlayFab.GroupsModels.EmptyResponse response)
    //{
    //    var prevRequest = (DeleteGroupRequest)response.Request;
    //    Debug.Log("Group Deleted: " + prevRequest.Group.Id);

    //    var temp = new HashSet<KeyValuePair<string, string>>();
    //    foreach (var each in EntityGroupPairs)
    //        if (each.Value != prevRequest.Group.Id)
    //            temp.Add(each);
    //    EntityGroupPairs.IntersectWith(temp);
    //    GroupNameById.Remove(prevRequest.Group.Id);
    //}

    //public void InviteToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    //{
    //    // A player-controlled entity invites another player-controlled entity to an existing group
    //    var request = new InviteToGroupRequest { Group = EntityKeyMaker(groupId, "group"), Entity = entityKey };
    //    PlayFabGroupsAPI.InviteToGroup(request, OnInvite, OnSharedError);
    //}
    //public void OnInvite(InviteToGroupResponse response)
    //{
    //    var prevRequest = (InviteToGroupRequest)response.Request;

    //    // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
    //    var request = new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id, "group"), Entity = prevRequest.Entity };
    //    PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);

    //    invUsers.SetActive(false);
    //}

    public void InviteToGroup()
    {
        // Read the display name of the player to be invited from the input field
        string invitedPlayerName = inviteplayerID.text;

        // Invite the player to the group
        InvitePlayerToGroup(invitedPlayerName);
    }

    private void InvitePlayerToGroup(string playerName)
    {
        // Get the PlayFab ID of the player based on display name
        var request = new GetAccountInfoRequest
        {
            TitleDisplayName = playerName
        };
        PlayFabClientAPI.GetAccountInfo(request, result => OnGetAccountInfoForDisplayNameForInvite(result, playerName), DisplayPlayFabError1);
    }

    private void OnGetAccountInfoForDisplayNameForInvite(GetAccountInfoResult result, string playerName)
    {
        // Check if the account info is available
        if (result.AccountInfo != null)
        {
            string invitedPlayerId = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id;
            // Now that you have the PlayFab ID, you can proceed to invite the player to the group
            var request = new InviteToGroupRequest
            {
                Group = EntityKeyMaker(userGroupID, "group"),
                Entity = EntityKeyMaker(invitedPlayerId, "title_player_account")
            };
            PlayFabGroupsAPI.InviteToGroup(request, OnInvite, result => Debug.Log(result));
        }
        else
        {
            Debug.Log("Player with the provided display name not found.");
            // Handle the case where the player with the given display name is not found
        }
    }

    public void OnInvite(InviteToGroupResponse response)
    {
        GameObject instantiatedObject = Instantiate(invitedUser);
        instantiatedObject.transform.SetParent(displayList.transform);
        instantiatedObject.transform.Find("Name").GetComponent<TMP_Text>().text = inviteplayerID.text;
    }

    //public void ApplyToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    //{
    //    // A player-controlled entity applies to join an existing group (of which they are not already a member)
    //    var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId, "group"), Entity = entityKey };
    //    PlayFabGroupsAPI.ApplyToGroup(request, OnApply, OnSharedError);
    //}
    //public void OnApply(ApplyToGroupResponse response)
    //{
    //    var prevRequest = (ApplyToGroupRequest)response.Request;

    //    // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
    //    var request = new AcceptGroupApplicationRequest { Group = prevRequest.Group, Entity = prevRequest.Entity };
    //    PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
    //}
    public void Back2Main()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OpenCreate()
    {
        createOwnGuild.SetActive(true);
    }

    public void CloseCreate()
    {
        createOwnGuild.SetActive(false);
    }

    public void OpenInvites()
    {
        invUsers.SetActive(true);
    }

    public void CloseInvites()
    {
        invUsers.SetActive(false);
    }

    public void OpenRequest()
    {
        GuildReq.SetActive(true);
    }

    public void CloseRequest()
    {
        GuildReq.SetActive(false);
    }

    void DisplayPlayFabError(PlayFabError error) { Debug.LogError("PlayFab API Error: " + error.GenerateErrorReport());}
    void DisplayPlayFabError1(PlayFabError error) { Debug.Log("u a nig"); }
}
