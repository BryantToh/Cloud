using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByContact : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    public int scoreValue;
    public int xpValue;
    GameController gameController;
    private void Start() {

        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if(gameControllerObject != null){
            gameController = gameControllerObject.GetComponent<GameController>();
        } 
        else{
            Debug.Log("GameController object not found");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag != "Boundary" && other.tag != "Asteroid"){
            Instantiate(explosion, transform.position, transform.rotation);

            if(other.tag == "Player"){
                Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
                gameController.gameIsOver();
                Destroy(other.gameObject);
            }
            else if(other.tag == "Barrier")
            {
                Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
                Destroy(other.gameObject);
            }
            gameController.addScore(scoreValue);
            gameController.addXP(xpValue);
            Destroy(gameObject);
        }
    }
}
