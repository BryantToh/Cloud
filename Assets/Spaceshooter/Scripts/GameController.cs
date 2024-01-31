using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour {

    public Vector3 positionAsteroid;
    public GameObject asteroid;
    public GameObject asteroid2;
    public GameObject asteroid3;
    public int hazardCount;
    public float startWait;
    public float spawnWait;
    public float waitForWaves;
    public Text scoreText;
    public Text gameOverText;
    public Text restartText;
    public Text mainMenuText;
    public TMP_Text XpText;
    public TMP_Text LevelText;
    public PlayFabUserMgtTMP playFabUser;
    public PFDataMgr playFabData;
    public MoneyManager moneyManager;
    public PlayerData playerData;
    public PlayerDataManager dataManager;

    private bool restart;
    private bool gameOver;
    private int score;
    private List<GameObject> asteroids;

    private void Start() {
        asteroids = new List<GameObject> {
            asteroid,
            asteroid2,
            asteroid3
        };
        gameOverText.text = "";
        restartText.text = "";
        mainMenuText.text = "";
        restart = false;
        gameOver = false;
        score = 0;
        StartCoroutine(spawnWaves());
        updateScore();
        updateXP();
        updateLevel();
        playFabData.GetUserData();
        dataManager.LoadJson();
    }

    private void Update() {
        if(restart){
            if(Input.GetKey(KeyCode.R)){
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            } 
            else if(Input.GetKey(KeyCode.Q)){
                SceneManager.LoadScene("Menu");
            }
        }

        if (gameOver) {
            restartText.text = "Press R to restart game";
            mainMenuText.text = "Press Q to go back to main menu";
            restart = true;
        }
    }

    private IEnumerator spawnWaves(){
        yield return new WaitForSeconds(startWait);
        while(true){
            for (int i = 0; i < hazardCount;i++){
                Vector3 position = new Vector3(Random.Range(-positionAsteroid.x, positionAsteroid.x), positionAsteroid.y, positionAsteroid.z);
                Quaternion rotation = Quaternion.identity;
                Instantiate(asteroids[Random.Range(0,3)], position, rotation);
                yield return new WaitForSeconds(spawnWait);
            }
            yield return new WaitForSeconds(waitForWaves);
            if(gameOver){
                break;
            }
        }
    }

    public void gameIsOver(){
        playFabUser.SendLeaderboard(score);
        moneyManager.intendedMoney = score;
        moneyManager.GainMoney();
        dataManager.SendJsonData();
        gameOverText.text = "Game Over";
        gameOver = true;
    }

    public void addScore(int score){
        this.score += score;
        updateScore();
    }

    public void addXP(int XP)
    {
        playFabData.playerexp += XP;
        updateXP();
        addLevel();
    }

    public void addLevel()
    {
        if (playFabData.playerexp >= 50)
        {
            playFabData.playerlevel++;
            playFabData.playerexp = 0;
            updateLevel();
        }
    }

    void updateScore(){
        scoreText.text = "Score:" + score;
    }

    void updateXP()
    {
        XpText.text = "XP: " + playFabData.playerexp;
    }

    void updateLevel()
    {
        LevelText.text = "Level: " + playFabData.playerlevel;
    }

    public PlayerData ReturnClass()
    {
        return new PlayerData(playFabData.playerexp, playFabData.playerlevel);
    }
}
