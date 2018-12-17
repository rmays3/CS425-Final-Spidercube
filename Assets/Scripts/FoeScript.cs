using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoeScript : MonoBehaviour {

    CharacterController characterController;

    private float speed = 15.0F;
    private Transform platTransform;
    private float platX;
    private float platZ;
    private Vector3 moveDir = Vector3.zero;
    private bool done = false;

	// Use this for initialization
	void Start () {
        characterController = GetComponent<CharacterController>();
        var plat = GameObject.FindGameObjectWithTag("Platform");
        if (!plat)
        {
            Debug.LogError(
              "The foe can't find any platforms.");
        }
        else
        {
            platTransform = plat.transform;
        }
        platX = platTransform.position.x + (platTransform.localScale.x / 2.0F);
        platZ = platTransform.position.z + (platTransform.localScale.z / 2.0F);
    }
	
	// Update is called once per frame
	void Update () {
        moveDir = Vector3.zero;
        //Debug.Log(characterController.transform.forward);
        turnCheck();
        moveDir = characterController.transform.forward;
        moveDir *= speed;
        
        characterController.Move(moveDir * Time.deltaTime);
    }

    private void turnCheck()
    {
        //float charX = characterController.transform.position.x - (characterController.transform.localScale.x / 2.0F);
        //float charZ = characterController.transform.position.z - (characterController.transform.localScale.z / 2.0F);
        Vector3 pos = characterController.transform.position - (characterController.transform.localScale / 2.0F);
        //pos.x = Mathf.Abs(pos.x);
        //pos.z = Mathf.Abs(pos.z);
        //Debug.Log(pos.z);
        //Debug.Log(platZ);
        if (pos.x >= platX && pos.z >= platZ && characterController.transform.eulerAngles.y == 0.0F)
        {
            pos = Vector3.zero;
            pos.y = 270.0F;
            characterController.transform.eulerAngles = pos;
        }
        else if(characterController.transform.position.x + (characterController.transform.localScale.x / 2.0F) < -platX && pos.z >= platZ && characterController.transform.eulerAngles.y == 270.0F)
        {
            pos = Vector3.zero;
            pos.y = 180.0F;
            characterController.transform.eulerAngles = pos;
        }
        else if(characterController.transform.position.x + (characterController.transform.localScale.x / 2.0F) < -platX && characterController.transform.position.z + (characterController.transform.localScale.z / 2.0F) < -platZ && characterController.transform.eulerAngles.y == 180.0F)
        {
            pos = Vector3.zero;
            pos.y = 90.0F;
            characterController.transform.eulerAngles = pos;
        }
        else if(pos.x >= platX && characterController.transform.position.z + (characterController.transform.localScale.z / 2.0F) < -platZ && characterController.transform.eulerAngles.y == 90.0F)
        {
            pos = Vector3.zero;
            characterController.transform.eulerAngles = pos;
        }
    }
}
