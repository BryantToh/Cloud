using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[System.Serializable]
public class Boundary{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerControl : MonoBehaviour {

    private Rigidbody playerRb;
    private AudioSource playerWeapon;
    public float speed;
    public float tiltMultiplier;
    public Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public Transform shotSpawn2;
    public float fireRate;

    public GameObject barrier;

    private float nextFire;
    private CharacterSelection characterSelection;
    private MoneyManager moneyManager;

    private void Start() {
        GameObject cSelectionObject = GameObject.FindWithTag("CharacterSelection");
        if (cSelectionObject != null) {
            characterSelection = cSelectionObject.GetComponent<CharacterSelection>();
        }

        GameObject mManagerObject = GameObject.FindWithTag("MoneyManager");
        if (mManagerObject != null)
        {
            moneyManager = mManagerObject.GetComponent<MoneyManager>();
        }

        CheckForBarrier();

        playerRb = GetComponent<Rigidbody>();
        playerWeapon = GetComponent<AudioSource>();
    }

    private void Update() {
        if(Input.GetButton("Jump") && Time.time > nextFire){
            nextFire = Time.time + fireRate;
            if(characterSelection.getIndex() == 1){
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
                Instantiate(shot, shotSpawn2.position, shotSpawn2.rotation);
            }
            else{
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            }
            playerWeapon.Play();
        }

    }

    private void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        playerRb.velocity = new Vector3(moveHorizontal * speed, 0.0f, moveVertical * speed);

        playerRb.position = new Vector3(
            Mathf.Clamp(playerRb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(playerRb.position.z, boundary.zMin, boundary.zMax)
        );

        playerRb.rotation = Quaternion.Euler(0.0f, 0.0f, -playerRb.velocity.x * tiltMultiplier);
    }

    private void CheckForBarrier()
    {
        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
        result =>
        {
            List<ItemInstance> inventory = result.Inventory;
            ItemInstance itemInstance = inventory.Find(item => item.ItemId == "Barrier");

            if (itemInstance != null && itemInstance.RemainingUses > 0)
            {
                activateBarrier(itemInstance);
                GameObject shield = Instantiate(barrier, transform.position, transform.rotation);
                shield.transform.parent = transform;
            }
        }, error => Debug.Log("no barrier"));
    }

    private void activateBarrier(ItemInstance item)
    {
        PlayFabClientAPI.ConsumeItem(new ConsumeItemRequest
        {
            ItemInstanceId = item.ItemInstanceId,
            ConsumeCount = 1
        },result => Debug.Log("barrier activated"), error=>Debug.Log(error.Error));
    }
}
