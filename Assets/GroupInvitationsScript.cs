using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.GroupsModels;

public class GroupInvitationsScript : MonoBehaviour
{
    [SerializeField] GameObject Display;
    [SerializeField] GameObject GroupToPlayerPrefab;
    string PlayerTitleID;
    List<string> InvitedGroupNames = new List<string>();
    private void Start()
    {
        GetUserAccountInfo();
    }
    public void GetUserAccountInfo()
    {
        var request = new GetAccountInfoRequest
        {
        };

        PlayFabClientAPI.GetAccountInfo(request, result => { PlayerTitleID = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id; }, Errorresult => { Debug.Log(Errorresult); });
    }

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
    void CheckInvite(ListGroupInvitationsResponse response)
    {
        int guildCount = 0;
        foreach (var invitation in response.Invitations)
        {
            //if invitations is for us
            if (invitation.InvitedEntity.Key.Id == PlayerTitleID)
            {
                //Make the prefab
                Debug.Log("Invited to group: " + invitation.Group.Id);
                GameObject groupInvitePrefab =  Instantiate(GroupToPlayerPrefab, Display.transform);
                groupInvitePrefab.GetComponent<GuildDataPrefab>().userGroupID = invitation.Group.Id;
                groupInvitePrefab.transform.Find("Guild Name").GetComponent<TMP_Text>().text = InvitedGroupNames[guildCount];
            }
            guildCount++;
        }
    }
    void CheckGroupInvitationsResult(GetGroupResponse response)
    {
        string GroupID = response.Group.Id;
        var checkGroupInvReq = new ListGroupInvitationsRequest
        {
            Group = new PlayFab.GroupsModels.EntityKey { Id = GroupID, Type = "group" }
        };
        //check if any of the invitation in for us
        PlayFabGroupsAPI.ListGroupInvitations(checkGroupInvReq, CheckInvite, result => Debug.Log(result));
       
    }
    private void ShowGroupInvitations()
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

                //Get all groups's ID using the name
                foreach (var guild in guildData.Groups)
                {
                    Debug.Log($"{guild.GroupName}");
                    InvitedGroupNames.Add(guild.GroupName);
                    guildNames += $"{guild.GroupName}\n";
                    var GrpReq = new GetGroupRequest { GroupName = guild.GroupName };
                    PlayFabGroupsAPI.GetGroup(GrpReq, CheckGroupInvitationsResult, result => Debug.Log(result));
                }
            }
            else
            {
                Debug.Log("Invalid or missing data in the JSON response.");
            }

        }, result => Debug.Log(result));
      

    }
    private void OnEnable()
    {
        ShowGroupInvitations();
    }
}
