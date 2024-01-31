using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayFabUserMgtTMP : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail/*, userPassword, userName, currentScore, displayName*/;
    [SerializeField] TextMeshProUGUI Msg;
    [SerializeField] TMP_Text LeaderBoardDisplay;

    private void Start()
    {
        OnButtonGetLeaderboard();
    }

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Msg.text=msg+'\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        SceneManager.LoadScene("Login");
    }

    public void OnButtonGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "Highscore", //playfab leaderboard statistic name
            StartPosition = 1,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGet, OnError);
    }

    public void MoveToLeaderBoard()
    {
        SceneManager.LoadScene("LearBoard");
    }

    public void BackBtn()
    {
        SceneManager.LoadScene("Menu");
    }

    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + ".   <" + item.DisplayName + "> | " + item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        updateLeaderBoardDisplay(LeaderboardStr);
    }

    void updateLeaderBoardDisplay(string e)
    {
        if (LeaderBoardDisplay != null)
        {
            LeaderBoardDisplay.text = e;
        }
    }

    public void SendLeaderboard(int newScore)
    {
        GetPlayerData(newScore);
    }

    void GetPlayerData(int newScore)
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            int currentHighScore = 0;
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == "Highscore")
                {
                    currentHighScore = stat.Value;
                    break;
                }
            }

            if (newScore > currentHighScore)
            {
                UpdatePlayerStatistics(newScore);
            }
            else
            {
                Debug.Log("New score is not higher than the current high score.");
            }
        }, OnError);
    }

    void UpdatePlayerStatistics(int newScore)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
        {
            new StatisticUpdate
            {
                StatisticName = "Highscore",
                Value = newScore,
            }
        }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderBoardUpdate, OnError);
    }

    void OnLeaderBoardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully sent leaderboard");
    }


    public void OnButtonGetLeaderboardLocal()
    {
        var llbreq = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Highscore", //playfab leaderboard statistic name
            MaxResultsCount = 5,
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(llbreq, OnLeaderboardGetLocal, OnError);
    }

    void OnLeaderboardGetLocal(GetLeaderboardAroundPlayerResult r)
    {
        string LeaderboardStr = "\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = (item.Position + 1) + ".   <" + item.DisplayName + "> | " + item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        updateLeaderBoardDisplay(LeaderboardStr);
    }

    public void EnterStore()
    {
        SceneManager.LoadScene("Store");
    }

    public void EnterFriends()
    {
        SceneManager.LoadScene("Friends");
    }

    public void EnterGuild()
    {
        SceneManager.LoadScene("Guild");
    }
}

