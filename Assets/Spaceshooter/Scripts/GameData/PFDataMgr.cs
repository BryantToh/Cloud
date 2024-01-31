using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFDataMgr : MonoBehaviour
{
    [SerializeField] TMP_Text XPDisplay;
    [SerializeField] TMP_Text LevelDisplay;
    [SerializeField]
    GameController gamecontroller;
    public List<PlayerControl> playerControlList = new List<PlayerControl>();
    public int playerexp;
    public int playerlevel;
    // Start is called before the first frame update

    public void SetUserData()
    {
        foreach (PlayerControl playerControl in playerControlList)
        {
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>()
            {
                { "Speed", playerControl.speed.ToString()},
                { "FireRate", playerControl.fireRate.ToString()},
            }
            },
            result => Debug.Log("Successfully updated user data"),
            error =>
            {
                Debug.Log("got error setting user data");
                Debug.Log(error.GenerateErrorReport());
            });
        }
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {

        },

        result =>

        {
            foreach (PlayerControl playerControl in playerControlList)
            {
                Debug.Log("Got user data: ");
                if (result.Data == null && result.Data.ContainsKey("PlayerData"))
                    Debug.Log("No XP");
                else
                {
                    playerexp = JsonUtility.FromJson<PlayerData>(result.Data["PlayerData"].Value).Exp;
                    playerlevel = JsonUtility.FromJson<PlayerData>(result.Data["PlayerData"].Value).Level;
                    if (XPDisplay != null && LevelDisplay.text != null)
                    {
                        XPDisplay.text = "Exp: " + playerexp;
                        LevelDisplay.text = "Level: " + playerlevel;
                    }
                }

                if (!result.Data.ContainsKey("Speed"))
                {
                    // If there is no health data, you can set a default value or handle it as needed.
                    playerControl.speed = 10;
                }
                else
                {
                    // Retrieve and set the player's health from the stored data.
                    if (int.TryParse(result.Data["Speed"].Value, out int storedSpeed))
                    {
                        playerControl.speed = storedSpeed;
                    }
                    else
                    {
                        Debug.LogError("Failed to parse speed data.");
                        playerControl.speed = 10;
                    }
                }

                if (!result.Data.ContainsKey("FireRate"))
                {
                    // If there is no health data, you can set a default value or handle it as needed.
                    playerControl.fireRate = 0.25f;
                }
                else
                {
                    // Retrieve and set the player's health from the stored data.
                    if (float.TryParse(result.Data["FireRate"].Value, out float storedfireRate))
                    {
                        playerControl.fireRate = storedfireRate;
                    }
                    else
                    {
                        Debug.LogError("Failed to parse Firerate data.");
                        playerControl.fireRate = 0.25f;
                    }
                }
            }
        },
        error =>
        {
            Debug.Log("Got error retrieving user data: ");
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
