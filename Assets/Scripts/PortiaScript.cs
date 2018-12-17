using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortiaScript : MonoBehaviour {

    CharacterController characterController;

    public float speed = 60.0F;

    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    public float camDistance = 10.0F;
    public Camera cam;
    //public GameObject ball;
    //public GameObject spawn;

    private bool isColliding = false;

    //All variables for adaptive camera movement via a "fairy"
    //The fairy is an invisible point that the camera is always looking at.  It's generally directed toward the player or center of stage.
    private bool fairyIsMoving = false;
    private Vector3 distTen;
    private int i = 0;
    private float fairyThreshold = 5.0F;
    private float fairySpeed = 40.0F;
    private Vector3 fairyDirection = Vector3.zero;
    //true if the fairy is at its target
    //false otherwise
    private bool fairyLock = true;
    //true if the fairy will look at the center of the platform
    //false if the fairy will look at the player
    private bool fairyAtCenter = true;

    private bool start = false;

    private string platName = "Bubbly Curly-Fry Steve";

    private Transform camTransform;
    private Transform thisTransform;

    //Variables used to control camera movement
    //We are using conventions in mathematics.
    private Vector3 camDistanceVector = Vector3.zero;
    private float theta;
    private float r;
    private float rho;
    private float phi;
    private bool angleChangeButtons = false;
    private float degree = 0;
    private Vector2 cameraLimits;
    private Vector3 sidesVisible = Vector3.zero;
    private float toTheta;
    private float toPhi;
    private bool transTheta = false;
    private bool transPhi = false;
    private float thetaInc;
    private float phiInc;
    private int tPhiCount = 0;
    private int tThetaCount = 0;
    private bool freeCamera = true;
    private bool autoAdjustOn = true;

    //Variables used to control relative direction during camera movement
    Vector3 relativeDir = Vector3.zero;
    Vector3 relativeDirPlusY = Vector3.zero;
    //Can be from 0 to 8
    //0 is no direction
    //1 is N, relative to rotation
    //2 is NE
    //3 is E
    //4 is SE
    //5 is S
    //6 is SW
    //7 is W
    //8 is NW
    private int cardDir = 0;
    private bool rotLock = false;

    //Variables to control rotation animation and handling
    private bool rotationStarted = false;
    private int rCount = 100;
    //-1: Negative side
    //1: Positive side
    //0: Not on the edge of this face
    private int xFace = 0;
    private int yFace = 0;
    private int zFace = 0;
    private int transitionFrames = 10;
    private Vector3 degRotTransition = Vector3.zero;
    private Vector3 placeTransition = Vector3.zero;
    private Vector3 pTOriginal = Vector3.zero;
    private Vector3 rTOriginal = Vector3.zero;
    private Vector3 trueRot = Vector3.zero;
    private bool noTransition = true;
    //We will not move if we have just done a side transition
    private bool sideTransed = false;
    //Side codes
    //  +X: 1
    //  +Y: 2
    //  +Z: 3
    //  -X: 4
    //  -Y: 5
    //  -Z: 6
    private int side = 2;
    private int prevSide = 2;
    private bool startedOnSide5 = false;
    private Vector2 startedOnXZSide = Vector2.zero;

    //Variables relating to game objectives
    private int eggCount = 0;
    private int eggGoal = 3;

    //Transform of the platform
    private Transform platTransform;

    //Transform of the fairy (it's an invisible icon that the camera follows!)
    private Transform fairyTransform;

    private Vector3 moveDirection = Vector3.zero;

    // Use this for initialization
    void Start () {
        characterController = GetComponent<CharacterController>();
        //anim = GetComponent<Animator>();
        var camera = GameObject.FindGameObjectWithTag("MainCamera");
        if (!camera)
        {
            Debug.LogError(
              "Could not find the main camera. Ensure it has the player tag set.");
        }

        

        else
        {
            // Grab a reference to its transform for use later (saves on managed
            // code to native code calls).
            camTransform = camera.transform;
        }

        var fair = GameObject.FindGameObjectWithTag("Fairy");
        if (!fair)
        {
            Debug.LogError(
              "Could not find the main camera. Ensure it has the player tag set.");
        }
        else
        {
            fairyTransform = fair.transform;
        }
        //camTransform = cam.transform;
        thisTransform = this.transform;
        //spawnTransform = spawn.transform;
        //sBody = ball.GetComponent<Rigidbody>();

        //*********************************************************  GIVING THETA, R, RHO AND PHI VALUES  *********************
        //camDistanceVector is constant through the program.
        camDistanceVector = camTransform.position - fairyTransform.position;
        //r is the length of the hypotenuse of the x and z components.
        r = Mathf.Sqrt((camTransform.position.x * camTransform.position.x) + (camTransform.position.z * camTransform.position.z));
        

        //Rho is the length of the vector from the camera to the fairy, which is a hypotenuse of its x, y and z components.
        rho = Mathf.Sqrt((camTransform.position.x * camTransform.position.x) + (camTransform.position.y * camTransform.position.y) + (camTransform.position.z * camTransform.position.z));

        //Theta is the angle that hypotenuse makes on the xz-axis.
        //Both theta and phi are in radians.  Remember that, now.
        theta = Mathf.Atan(camTransform.position.z / rho);

        //Phi is the angle the hypotenuse makes on the yz-axis.
        phi = Mathf.Acos(camTransform.position.y / rho);

        cameraLimits = new Vector2(5.0F, 20.0F);
        sidesVisibleHandler();
        Debug.Log("Auto-adjust is ON.");
    }

    // Update is called once per frame
    void Update () {

        if (rotationStarted)
        {
            if(rCount < transitionFrames){
                if(rCount == 0)
                {
                    trueRot = characterController.transform.eulerAngles;
                    
                }
                if (rCount == transitionFrames - 1)
                {

                    
                    characterController.transform.eulerAngles = rTOriginal;
                    characterController.transform.position = pTOriginal;
                }
                else
                {
                    characterController.transform.position += placeTransition;

                    Vector3 v = trueRot + degRotTransition;
                    trueRot += degRotTransition;
                    characterController.transform.eulerAngles = v;
                }
                rCount++;
            }
            else
            {
                rotationStarted = false;
                rCount = 100;
                noTransition = true;
                sideTransed = true;
            }
        }
        else if (start && !rotationStarted)
        {
            //*****************************************************************************  EDGE HANDLING  *****************************
            //If we're on an edge:
            //Go to the edge handler.  If our direction is good, we'll start going to the next edge.
            //This is where start might end for a while, as we will rotate for a little bit as a transition.  It's quite brief, though.
            if (Input.GetButtonDown("Jump"))
            {
                if (autoAdjustOn)
                {
                    autoAdjustOn = false;
                    Debug.Log("Auto-adjust is OFF.");
                }
                else
                {
                    autoAdjustOn = true;
                    Debug.Log("Auto-adjust is ON.");
                }
            }
            if (isOnEdge())
            {
                edgeHandler();
            }
            if(noTransition)
            {
                prevSide = side;

                // We are grounded, so recalculate
                // move direction directly from axes
                //W: Vertical >0.00F
                //D: Horizontal >0.00F
                //*****************************************************************************  DIRECTIONS  ********************************
                //If we let go of the move key, we let go of our relative rotation/direction.
                if (!isMoveKeyHeld())
                {
                    rotLock = false;
                    cardDir = -1;
                }
                else
                {
                    sideTransed = false;
                }
                //W is pressed only
                if (Input.GetAxis("Vertical") > 0.00F && Input.GetAxis("Horizontal") == 0.00F)
                {
                    if (cardDir != 1)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 1;
                    
                }
                //W and A are pressed
                else if (Input.GetAxis("Vertical") > 0.00F && Input.GetAxis("Horizontal") < 0.00F)
                {
                    if (cardDir != 8)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y -= 45.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 8;
                    
                }
                //W and D are pressed
                else if (Input.GetAxis("Vertical") > 0.00F && Input.GetAxis("Horizontal") > 0.00F)
                {
                    if (cardDir != 2)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y += 45.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 2;
                    
                }
                //S is pressed only
                else if (Input.GetAxis("Vertical") < 0.00F && Input.GetAxis("Horizontal") == 0.00F)
                {
                    if (cardDir != 5)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y += 180.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 5;
                    
                }
                //S and A are pressed
                else if (Input.GetAxis("Vertical") < 0.00F && Input.GetAxis("Horizontal") < 0.00F)
                {
                    if (cardDir != 6)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y -= 135.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 6;
                    
                }
                //S and D are pressed
                else if (Input.GetAxis("Vertical") < 0.00F && Input.GetAxis("Horizontal") > 0.00F)
                {
                    if (cardDir != 4)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y += 135.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 4;
                    
                }
                //D is pressed only.
                else if (Input.GetAxis("Vertical") == 0.00F && Input.GetAxis("Horizontal") > 0.00F)
                {
                    if (cardDir != 3)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y += 90.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }
                    cardDir = 3;
                    
                }
                //A is pressed only.
                else if (Input.GetAxis("Vertical") == 0.00F && Input.GetAxis("Horizontal") < 0.00F)
                {
                    if (cardDir != 7)
                    {
                        rotLock = false;
                    }
                    else
                    {
                        rotLock = true;
                    }
                    Vector3 camAngles = camTransform.rotation.eulerAngles;
                    camAngles.x = 0.0F;
                    camAngles.y -= 90.0F;
                    camAngles.z = 0.0F;
                    Vector3 camAngles2 = rotHandler(camAngles);
                    if (rotLock)
                    {
                        characterController.transform.eulerAngles = relativeDir;
                    }
                    else
                    {
                        characterController.transform.eulerAngles = camAngles2;
                        relativeDir = camAngles2;
                        relativeDirPlusY = camAngles;
                    }

                    cardDir = 7;
                    
                }
                if (sideTransed)
                {
                    moveDirection = new Vector3(0.0F, 0.0F, 0.0F);
                }
                else
                {
                    moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
                }
                

                if (cardDir == 5)
                {
                    moveDirection.z *= -1.0F;
                }
                else if (cardDir == 7)
                {
                    moveDirection.z = moveDirection.x * (-1.0F);
                    moveDirection.x = 0.0F;
                }
                else if (cardDir == 3)
                {
                    moveDirection.z = moveDirection.x * (1.0F);
                    moveDirection.x = 0.0F;
                }
                else if (cardDir % 2 == 0 && cardDir != 0)
                {
                    if (Mathf.Abs(moveDirection.z) >= Mathf.Abs(moveDirection.x))
                    {
                        moveDirection.x = moveDirection.z;
                    }
                    else
                    {
                        moveDirection.z = moveDirection.x;
                    }

                    moveDirection.x = 0.0F;
                    moveDirection.z = Mathf.Abs(moveDirection.z);
                }
                moveDirection = transform.TransformDirection(moveDirection);

                moveDirection *= speed;

                if (moveDirection.Equals(Vector3.zero))
                {
                    //anim.SetTrigger("StartIdle");
                }
                else
                {
                    //anim.SetTrigger("StartWalk");
                }

                if (Input.GetButton("Jump"))
                {
                    //do nothing lol
                }
                // Move the controller
                characterController.Move(moveDirection * Time.deltaTime);
            }
            
            
        }
        //Gravity is applied at the start of the level if we are above thin air
        if (!start)
        {
            moveDirection.y -= gravity * Time.deltaTime;
            characterController.Move(moveDirection * Time.deltaTime);
        }
        

        


    }
    //Helps us keep relative direction during camera change
    private bool isMoveKeyHeld()
    {
        if(Input.GetAxis("Horizontal") != 0.00F || Input.GetAxis("Vertical") != 0.00F)
        {
            return true;
        }
        return false;
    }

    private bool isOnEdge()
    {
        xFace = 0;
        yFace = 0;
        zFace = 0;
        Vector3 charPosOrigin = (characterController.transform.position - platTransform.position);
        if(charPosOrigin.x < 0.0F)
        {
            charPosOrigin.x *= -1.0F;
        }
        if (charPosOrigin.y < 0.0F)
        {
            charPosOrigin.y *= -1.0F;
        }
        if (charPosOrigin.z < 0.0F)
        {
            charPosOrigin.z *= -1.0F;
        }

        Vector3 cMinusRad = charPosOrigin - (characterController.transform.localScale / 2.0F);
        if (cMinusRad.x < 0.0F)
        {
            cMinusRad.x *= -1.0F;
        }
        if (cMinusRad.y < 0.0F)
        {
            cMinusRad.y *= -1.0F;
        }
        if (cMinusRad.z < 0.0F)
        {
            cMinusRad.z *= -1.0F;
        }
        Vector3 pRad = platTransform.localScale / 2.0F;
        int numTrue = 0;
        float epsilon = 0.04F;
        
        if(Mathf.Abs(charPosOrigin.x - Mathf.Abs(pRad.x)) <= epsilon || Mathf.Abs(cMinusRad.x - Mathf.Abs(pRad.x)) <= epsilon)
        {

            numTrue++;
            if ((characterController.transform.position.x - platTransform.position.x) < 0)
            {
                xFace = -1;
            }
            else
            {
                xFace = 1;
            }
        }
        if (Mathf.Abs(charPosOrigin.y - Mathf.Abs(pRad.y)) <= epsilon || Mathf.Abs(cMinusRad.y - Mathf.Abs(pRad.y)) <= epsilon)
        {

            numTrue++;
            if ((characterController.transform.position.y - platTransform.position.y) < 0)
            {
                yFace = -1;
            }
            else
            {
                yFace = 1;
            }
        }
        if (Mathf.Abs(charPosOrigin.z - Mathf.Abs(pRad.z)) <= epsilon || Mathf.Abs(cMinusRad.z - Mathf.Abs(pRad.z)) <= epsilon)
        {

            numTrue++;
            if ((characterController.transform.position.z - platTransform.position.z) < 0)
            {
                zFace = -1;
            }
            else
            {
                zFace = 1;
            }
        }

        return (numTrue >= 2);
    }

    private Vector3 rotHandler(Vector3 rot)
    {


        Vector3 v2 = rot;
        //+- X
        if (side == 1)
        {
            v2.x = rot.y;
            v2.y = 0.0F;
            v2.z = -90.0F;

            if ((isMoveKeyHeld() && Mathf.Abs(rot.y % 180.0F) > 0.1F) && rotLock && startedOnSide5)
            {
                v2.x = -rot.y;
            }

            if (!rotLock)
            {
                startedOnSide5 = false;
                startedOnXZSide.x = 1;

                if ((Mathf.Abs(v2.x) % 360) < 180.0)
                {
                    
                    startedOnXZSide.y = 2;
                }
                else
                {
                    startedOnXZSide.y = 1;
                }

            }
        }
        else if(side == 4)
        {
            v2.x = -rot.y;
            v2.y = 0.0F;
            v2.z = 90.0F;
            if ((isMoveKeyHeld() && Mathf.Abs(rot.y % 180.0F) > 0.1F) && rotLock && startedOnSide5)
            {
                v2.x = rot.y;
            }

            if (rotLock && startedOnXZSide.x != 4 && startedOnXZSide.x != 0 && startedOnXZSide.y != 0)
            {
                if (startedOnXZSide.y == 1)
                {
                    v2.x = (90 - v2.x);
                }
                else
                {
                    v2.x = 180.0F + v2.x;
                }
            }

            if (!rotLock)
            {
                startedOnSide5 = false;
                startedOnXZSide.x = 4;

                if ((Mathf.Abs(v2.x) % 360) > 180.0)
                {

                    startedOnXZSide.y = 2;
                }
                else
                {
                    startedOnXZSide.y = 1;
                }

            }
        }
        //+- Y
        else if (side == 2)
        {
            v2 = rot;

            if ((isMoveKeyHeld() && Mathf.Abs(rot.y % 180.0F) > 0.1F) && rotLock && startedOnSide5)
            {
                v2.y = -rot.y;

            }
            if ((isMoveKeyHeld() && (Mathf.Abs(rot.y % 180.0F) <= 89.9F || Mathf.Abs(rot.y % 180.0F) >= 91.1F)) && rotLock && startedOnSide5)
            {

                v2.y = 180 - rot.y;
            }

            if (!rotLock)
            {
                startedOnSide5 = false;
                startedOnXZSide.x = 0;
            }
        }
        else if(side == 5)
        {

            v2.z = 180;

            if ((isMoveKeyHeld() && Mathf.Abs(rot.y % 180.0F) > 0.1F) && rotLock && !startedOnSide5)
            {

                v2.y = -1.0F * rot.y;
                
            }

            if ((isMoveKeyHeld() && (Mathf.Abs(rot.y % 180.0F) <= 89.9F || Mathf.Abs(rot.y % 180.0F) >= 91.1F)) && rotLock && !startedOnSide5)
            {

                v2.y = rot.y + 180;

            }

            if (!rotLock)
            {
                startedOnSide5 = true;
                startedOnXZSide.x = 0;
            }

        }
        //+- Z
        else if (side == 3)
        {
            v2.z = 90.0F;
            v2.y = 90.0F;
            v2.x = 90 + rot.y;
            
            //W or D is pressed at a certain angle
            if (Input.GetAxis("Vertical") != 0.0F)
            {
                if (camTransform.forward.x < 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
                else
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
            }

            if (Input.GetAxis("Horizontal") != 0.0F)
            {
                if (camTransform.forward.z < 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
                else if (camTransform.forward.z > 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
            }
            
            if (Input.GetAxis("Horizontal") != 0.0F && Input.GetAxis("Vertical") != 0.0F)
            {
                v2.z *= -1.0F;
                v2.y *= -1.0F;
            }
            
            if ((isMoveKeyHeld() && (Mathf.Abs(rot.y % 180.0F) <= 89.9F || Mathf.Abs(rot.y % 180.0F) >= 91.1F)) && rotLock && startedOnSide5)
            {
                v2.x *= -1.0F;
            }

            if(rotLock && startedOnXZSide.x != 3 && startedOnXZSide.x != 0 && startedOnXZSide.y != 0)
            {
                if(startedOnXZSide.y == 1)
                {
                    v2.y *= -1.0F;
                    v2.z *= -1.0F;
                    
                    v2.x = (90 - v2.x);
                }
                else
                {
                    v2.y *= -1.0F;
                    v2.z *= -1.0F;
                    v2.x = -(90 + v2.x);
                }
            }


            if (!rotLock)
            {
                startedOnSide5 = false;
                startedOnXZSide.x = 3;

                float f = v2.x - 180.0F;
                if ((Mathf.Abs(f) % 360) > 180.0)
                {

                    startedOnXZSide.y = 1;
                }
                else
                {
                    startedOnXZSide.y = 2;
                }
            }
        }
        else if(side == 6)
        {
            v2.z = 90.0F;
            v2.y = -90.0F;
            v2.x = rot.y - 90.0F;

            //W or D is pressed at a certain angle
            if(Input.GetAxis("Vertical") != 0.0F)
            {
                if(camTransform.forward.x < 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
                else
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
            }

            if(Input.GetAxis("Horizontal") != 0.0F)
            {
                if (camTransform.forward.z < 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
                else if (camTransform.forward.z > 0.0F)
                {
                    v2.z *= -1.0F;
                    v2.y *= -1.0F;
                }
            }

            if(Input.GetAxis("Horizontal") != 0.0F && Input.GetAxis("Vertical") != 0.0F)
            {
                v2.z *= -1.0F;
                v2.y *= -1.0F;
            }

            if ((isMoveKeyHeld() && (Mathf.Abs(rot.y % 180.0F) <= 89.9F || Mathf.Abs(rot.y % 180.0F) >= 91.1F)) && rotLock && startedOnSide5)
            {
                v2.x *= -1.0F;
            }

            if (rotLock && startedOnXZSide.x != 6 && startedOnXZSide.x != 0 && startedOnXZSide.y != 0)
            {
                if (startedOnXZSide.y == 1)
                {
                    v2.x = (90 - v2.x);
                }
                else
                {
                    
                    v2.y = 90.0F;
                    v2.z = -90.0F;
                    v2.x = v2.x + 90.0F;
                }
            }

            if (!rotLock)
            {
                startedOnSide5 = false;
                startedOnXZSide.x = 6;
                if ((Mathf.Abs(v2.x) % 360) < 180.0)
                {

                    startedOnXZSide.y = 2;
                }
                else
                {
                    startedOnXZSide.y = 1;
                }
            }
        }
        return v2;
    }
    //Given a vector of direction, adjust it whether you are on the x, y or z faces.
    private Vector3 dirHandler(Vector3 dir)
    {
        Vector3 v2 = dir;
        //+- X
        if(side == 1 || side == 4)
        {
            v2.y = dir.x;
            v2.x = 0.0F;
        }
        //+- Y
        else if (side == 2 || side == 5)
        {
            v2 = dir;
        }
        //+- Z
        else if (side == 3 || side == 6)
        {
            v2.y = dir.z;
            v2.z = 0.0F;
        }
        return v2;
    }

    private void edgeHandler()
    {
        Quaternion q = Quaternion.identity;
        Vector3 wdir = q * characterController.transform.forward;
        Vector3 v1 = characterController.transform.rotation.eulerAngles;
        Vector3 v2 = Vector3.zero;
        Vector3 v3 = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        int sum = 0;


        //Comparison of decimals is tough.  If the decimal is small enough, we'll treat it as 0.
        //Without this code, our character would spin in circles on any edge!
        float epsilon = 0.02F;
        if(Mathf.Abs(wdir.x) < epsilon)
        {
            wdir.x = 0;
        }
        if (Mathf.Abs(wdir.y) < epsilon)
        {
            wdir.y = 0;
        }
        if (Mathf.Abs(wdir.z) < epsilon)
        {
            wdir.z = 0;
        }

        //How many sides are we on the edge of?
        if (xFace != 0)
            sum++;
        if (yFace != 0)
            sum++;
        if (zFace != 0)
            sum++;

        if (sum > 0)
        {
            if(xFace == 1 && yFace == 1)
            {
                //+Y face to +X face
                if (wdir.x > 0.0F)
                {
                    v2.x = v1.y;
                    v2.z -= 90.0F;
                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x + (platTransform.localScale.x / 2.0F)) + (characterController.transform.localScale.x / 2.0F);
                    placeTransition.y = platTransform.position.y + (platTransform.localScale.y / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 1;

                    degRotTransition = v3;              
                    noTransition = false;
                }
                //+X face to +Y face
                else if (wdir.y > 0.0F)
                {
                    v2.y = v1.x;
                    v2.z = 0.0F;
                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y + (platTransform.localScale.y / 2.0F)) + (characterController.transform.localScale.y / 2.0F);
                    placeTransition.x = platTransform.position.x + (platTransform.localScale.x / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 2;

                    degRotTransition = v3;
                    noTransition = false;                 //rot = Quaternion.FromToRotation(v1, v2);
                }
                else
                {
                    noTransition = true;
                }
                
            }
            
            else if (xFace == 1 && zFace == 1)
            {
                //+Z to +X
                if (wdir.x > 0.0F)
                {
                    v2.z = v1.z;
                    v2.y = 0.0F;
                    v2.x = 90 - v1.x;
                    
                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x + (platTransform.localScale.x / 2.0F)) + (characterController.transform.localScale.x / 2.0F);
                    placeTransition.z = platTransform.position.z + (platTransform.localScale.z / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 1;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //+X to +Z
                else if (wdir.z > 0.0F)
                {
                    v2.z = -90.0F;
                    v2.y = -90.0F;
                    v2.x = v1.x;
                    if (wdir.y < 0.0F)
                    {
                        v2.z = 90.0F;
                        v2.y = 90.0F;
                        v2.x = v1.x;
                    }

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z + (platTransform.localScale.z / 2.0F)) + (characterController.transform.localScale.z / 2.0F);
                    placeTransition.x = platTransform.position.x + (platTransform.localScale.x / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 3;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == 1 && yFace == -1)
            {
                //-Y to +X
                if (wdir.x > 0.0F)
                {
                    v2.x = -v1.y;
                    v2.z -= 90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x + (platTransform.localScale.x / 2.0F)) + (characterController.transform.localScale.x / 2.0F);
                    placeTransition.y = platTransform.position.y - (platTransform.localScale.y / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 1;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //+X to -Y
                else if (wdir.y < 0.0F)
                {
                    v2.y = -v1.x;
                    v2.x = 0.0F;
                    v2.z -= 180.0F;
                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y - (platTransform.localScale.y / 2.0F)) - (characterController.transform.localScale.y / 2.0F);
                    placeTransition.x = platTransform.position.x + (platTransform.localScale.x / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 5;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == 1 && zFace == -1)
            {
                //-Z to +X
                if (wdir.x > 0.0F)
                {
                    v2.x = v1.x;
                    v2.z = -90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x + (platTransform.localScale.x / 2.0F)) + (characterController.transform.localScale.x / 2.0F);
                    placeTransition.z = platTransform.position.z - (platTransform.localScale.z / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 1;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //+X to -Z
                else if (wdir.z < 0.0F)
                {
                    v2.z = 90.0F;
                    v2.y = -90.0F;
                    v2.x = v1.x;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z - (platTransform.localScale.z / 2.0F)) - (characterController.transform.localScale.z / 2.0F);
                    placeTransition.x = platTransform.position.x + (platTransform.localScale.x / 2.0F);
                    placeTransition.y = characterController.transform.position.y;
                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 6;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == -1 && yFace == 1)
            {

                
                //+Y to -X
                if (wdir.x < 0.0F)
                {
                    v2.x = -v1.y;
                    v2.z += 90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x - (platTransform.localScale.x / 2.0F)) - (characterController.transform.localScale.x / 2.0F);
                    placeTransition.y = platTransform.position.y + (platTransform.localScale.y / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 4;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-X to +Y
                else if (wdir.y > 0.0F)
                {
                    v2.y = -v1.x;
                    v2.z = 0.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y + (platTransform.localScale.y / 2.0F)) + (characterController.transform.localScale.y / 2.0F);
                    placeTransition.x = platTransform.position.x - (platTransform.localScale.x / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 2;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == -1 && zFace == 1)
            {
                //+Z to -X
                if (wdir.x < 0.0F)
                {
                    //v2.x = v1.z;
                    //v2.y -= 90.0F;
                    v2.z = 90.0F;
                    v2.y = 0.0F;
                    v2.x = 180 + v1.x;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x - (platTransform.localScale.x / 2.0F)) - (characterController.transform.localScale.x / 2.0F);
                    placeTransition.z = platTransform.position.z + (platTransform.localScale.z / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 4;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-X to +Z
                else if (wdir.z > 0.0F)
                {
                    v2.z = 90.0F;
                    v2.y = 90.0F;
                    v2.x = v1.x;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z + (platTransform.localScale.z / 2.0F)) + (characterController.transform.localScale.z / 2.0F);
                    placeTransition.x = platTransform.position.x - (platTransform.localScale.x / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 3;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == -1 && yFace == -1)
            {
                //-Y to -X
                if (wdir.x < 0.0F)
                {
                    v2.x = v1.y;
                    v2.z += 90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x - (platTransform.localScale.x / 2.0F)) - (characterController.transform.localScale.x / 2.0F);
                    placeTransition.y = platTransform.position.y - (platTransform.localScale.y / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 4;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-X to -Y
                else if (wdir.y < 0.0F)
                {
                    v2.x = -v1.y;
                    v2.z -= 90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y - (platTransform.localScale.y / 2.0F)) - (characterController.transform.localScale.y / 2.0F);
                    placeTransition.x = platTransform.position.x - (platTransform.localScale.x / 2.0F);
                    placeTransition.z = characterController.transform.position.z;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 5;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (xFace == -1 && zFace == -1)
            {
                //-Z to -X
                if (wdir.x < 0.0F)
                {
                    v2.x = v1.x;
                    v2.z = 90.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.x = (platTransform.position.x - (platTransform.localScale.x / 2.0F)) - (characterController.transform.localScale.x / 2.0F);
                    placeTransition.z = platTransform.position.z - (platTransform.localScale.z / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 4;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-X to -Z
                else if (wdir.z < 0.0F)
                {
                    v2.z = -90.0F;
                    v2.y = 90.0F;
                    v2.x = v1.x;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z - (platTransform.localScale.z / 2.0F)) - (characterController.transform.localScale.z / 2.0F);
                    placeTransition.x = platTransform.position.x - (platTransform.localScale.x / 2.0F);
                    placeTransition.y = characterController.transform.position.y;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 6;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            
            else if (yFace == 1 && zFace == 1)
            {
                
                //+Z to +Y
                if (wdir.y > 0.0F)
                {
                    v2.x = 0.0F;
                    v2.z = 0.0F;
                    v2.y = 90 - v1.x;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y + (platTransform.localScale.y / 2.0F)) + (characterController.transform.localScale.y / 2.0F);
                    placeTransition.z = platTransform.position.z + (platTransform.localScale.z / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 2;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //+Y to +Z
                else if (wdir.z > 0.0F)
                {
                    v2.z = 90.0F;
                    v2.y = 90.0F;
                    v2.x = v1.y;
                    if (wdir.x < 0.0F)
                    {
                        v2.z = -90.0F;
                        v2.y = -90.0F;
                        v2.x = -v1.y;
                    }

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z + (platTransform.localScale.z / 2.0F)) + (characterController.transform.localScale.z / 2.0F);
                    placeTransition.y = platTransform.position.y + (platTransform.localScale.y / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 3;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (yFace == 1 && zFace == -1)
            {
                //-Z to +Y
                //We'll need to have wdir.x when translating from z to somewhere else! Weird, huh?
                if (wdir.y > 0.0F)
                {
                    v2.y = 270 - v1.x;
                    v2.x = 0.0F;
                    v2.z = 0.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y + (platTransform.localScale.y / 2.0F)) + (characterController.transform.localScale.y / 2.0F);
                    placeTransition.z = platTransform.position.z - (platTransform.localScale.z / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 2;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //+Y to -Z
                else if (wdir.z < 0.0F)
                {
                    v2.z = 90.0F;
                    v2.y = -90.0F;
                    v2.x = v1.y;
                    if(wdir.x < 0.0F)
                    {
                        v2.z = -90.0F;
                        v2.y = 90.0F;
                        v2.x = -v1.y;
                    }

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z - (platTransform.localScale.z / 2.0F)) - (characterController.transform.localScale.z / 2.0F);
                    placeTransition.y = platTransform.position.y + (platTransform.localScale.y / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 6;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (yFace == -1 && zFace == 1)
            {
                //+Z to -Y
                if (wdir.y < 0.0F)
                {
                    v2.y = v1.x + 90.0F;
                    v2.x = 0.0F;
                    v2.z = 180.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y - (platTransform.localScale.y / 2.0F)) - (characterController.transform.localScale.y / 2.0F);
                    placeTransition.z = platTransform.position.z + (platTransform.localScale.z / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 5;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-Y to +Z
                else if (wdir.z > 0.0F)
                {

                    v2.z = -90.0F;
                    v2.y = -90.0F;
                    v2.x = -(90.0F + v1.y);
                    
                    if (wdir.y > 0.0F)
                    {
                        v2.z = 90.0F;
                        v2.y = 90.0F;
                        v2.x = -(v1.y - 90.0F);
                    }

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z + (platTransform.localScale.z / 2.0F)) + (characterController.transform.localScale.z / 2.0F);
                    placeTransition.y = platTransform.position.y - (platTransform.localScale.y / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 3;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            else if (yFace == -1 && zFace == -1)
            {
                //-Z to -Y
                if (wdir.y < 0.0F)
                {
                    v2.y = 90.0F - v1.x;
                    v2.x = 0.0F;
                    v2.z = 180.0F;

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.y = (platTransform.position.y - (platTransform.localScale.y / 2.0F)) - (characterController.transform.localScale.y / 2.0F);
                    placeTransition.z = platTransform.position.z - (platTransform.localScale.z / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 5;

                    degRotTransition = v3;
                    noTransition = false;
                }
                //-Y to -Z
                else if (wdir.z < 0.0F)
                {
                    v2.z = -90.0F;
                    v2.y = 90.0F;
                    v2.x = 90.0F - v1.y;
                    if (wdir.y > 0.0F)
                    {
                        v2.z = 90.0F;
                        v2.y = -90.0F;
                        v2.x = 90.0F - v1.y;
                    }

                    v3 = v2 - v1;
                    rTOriginal = v2;
                    v3 /= transitionFrames;

                    placeTransition = Vector3.zero;
                    placeTransition.z = (platTransform.position.z - (platTransform.localScale.z / 2.0F)) - (characterController.transform.localScale.z / 2.0F);
                    placeTransition.y = platTransform.position.y - (platTransform.localScale.y / 2.0F);
                    placeTransition.x = characterController.transform.position.x;

                    pTOriginal = placeTransition;
                    placeTransition = placeTransition - characterController.transform.position;
                    placeTransition /= transitionFrames;

                    side = 6;

                    degRotTransition = v3;
                    noTransition = false;
                }
                else
                {
                    noTransition = true;
                }
            }
            if(sum > 1 && !noTransition)
            {
                rotationStarted = true;
                rCount = 0;
                relativeDir = rotHandler(relativeDirPlusY);
            }
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Platform" && platName != other.gameObject.name)
        {
            platTransform = other.gameObject.transform;
            platName = other.gameObject.name;
            isColliding = true;
            gravity = 0.0F;
            start = true;
        }
        else if(other.gameObject.tag == "Egg")
        {
            eggCount++;
            Destroy(other.gameObject);
            if(eggCount == eggGoal)
            {
                Debug.Log("You win!");
                eggCount++;
            }
        }
        else if(other.gameObject.tag == "Foe")
        {
            Debug.Log("Game over!");
            Destroy(this.gameObject);
        }
    }

    private void sidesVisibleHandler()
    {
        if(camTransform.forward.x < 0.0)
        {
            sidesVisible.x = 1;
        }
        else
        {
            sidesVisible.x = 4;
        }
        if(camTransform.forward.y < 0.0)
        {
            sidesVisible.y = 2;
        }
        else
        {
            sidesVisible.y = 5;
        }
        if(camTransform.forward.z < 0.0)
        {
            sidesVisible.z = 3;
        }
        else
        {
            sidesVisible.z = 6;
        }
    }

    private void sideUpdate()
    {
        if(sidesVisible.x == 1 && side == 4 && autoAdjustOn)
        {
            if(sidesVisible.z == 3)
            {
                toTheta = Mathf.PI * (2.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            else if (sidesVisible.z == 6)
            {
                toTheta = Mathf.PI * (4.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            tThetaCount = 0;
        }
        else if(sidesVisible.x == 4 && side == 1 && autoAdjustOn)
        {
            if (sidesVisible.z == 3)
            {
                toTheta = Mathf.PI * (1.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            else if (sidesVisible.z == 6)
            {
                toTheta = Mathf.PI * (5.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            tThetaCount = 0;
        }
        else if (sidesVisible.y == 2 && side == 5 && autoAdjustOn)
        {
            toPhi = Mathf.PI * (2.0F / 3.0F);
            transPhi = true;
            phiInc = (toPhi - phi) / 50.0F;
            tPhiCount = 0;
        }
        else if(sidesVisible.y == 5 && side == 2 && autoAdjustOn)
        {
            toPhi = Mathf.PI * (1.0F / 3.0F);
            transPhi = true;
            phiInc = (toPhi - phi) / 50.0F;
            tPhiCount = 0;
        }
        else if (sidesVisible.z == 3 && side == 6 && autoAdjustOn)
        {
            if (sidesVisible.x == 1)
            {
                toTheta = Mathf.PI * (5.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            else if (sidesVisible.x == 4)
            {
                toTheta = Mathf.PI * (4.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            tThetaCount = 0;
        }
        else if(sidesVisible.z == 6 && side == 3 && autoAdjustOn)
        {
            if (sidesVisible.x == 1)
            {
                toTheta = Mathf.PI * (1.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
            }
            else if (sidesVisible.x == 4)
            {
                toTheta = Mathf.PI * (2.0F / 3.0F);
                transTheta = true;
                thetaInc = (toTheta - theta) / 50.0F;
                
            }
            tThetaCount = 0;
        }
    }

    private void LateUpdate()
    {
        theta %= (2 * Mathf.PI);
        phi %= (2 * Mathf.PI);
        Vector3 prevSides = sidesVisible;
        sidesVisibleHandler();
        
        if (prevSide == side)
        {
            freeCamera = true;
        }
        else
        {
            sideUpdate();
            freeCamera = false;
        }
        
        //Debug.Log(freeCamera);
        if (fairyIsMoving)
        {
            if (i < 35)
            {
                i++;
            }
            else
            {
                fairyIsMoving = false;
            }
        }
        else
        {
            
            Vector3 result = Vector3.zero;
            if (start)
            {
                if (characterController.transform.position.x >= fairyThreshold + platTransform.position.x || characterController.transform.position.y >= fairyThreshold + platTransform.position.y || characterController.transform.position.z >= fairyThreshold + platTransform.position.z || characterController.transform.position.x <= -fairyThreshold - platTransform.position.x || characterController.transform.position.y <= -fairyThreshold - platTransform.position.x || characterController.transform.position.z <= -fairyThreshold - platTransform.position.x)
                {
                    if (fairyAtCenter)
                    {
                        fairyAtCenter = false;
                        fairyLock = false;
                    }
                    //If we are not at the player, the fairy travels a little there
                    if (fairyTransform.position != characterController.transform.position && (!fairyLock))
                    {
                        var moveAmount = fairySpeed * Time.deltaTime;
                        fairyTransform.position = Vector3.MoveTowards(fairyTransform.position, characterController.transform.position, moveAmount);
                    }
                    else
                    {
                        fairyTransform.position = characterController.transform.position;
                        fairyLock = true;
                    }
                    //Else, the fairy is at the player's position
                }
                //Otherwise, go to the center if we aren't already
                //Otherwise otherwise, stay at the center
                else
                {
                    if (!fairyAtCenter)
                    {
                        fairyAtCenter = true;
                        fairyLock = false;
                    }
                    if (fairyTransform.position != platTransform.position && (!fairyLock))
                    {
                        var moveAmount = fairySpeed * Time.deltaTime;
                        fairyTransform.position = Vector3.MoveTowards(fairyTransform.position, platTransform.position, moveAmount);
                    }
                    else
                    {
                        fairyTransform.position = platTransform.position;
                        fairyLock = true;
                    }
                }
            }
        }
        //*************************************************  ANGLE CHANGE ON INPUT  **********************************

        if(Input.GetAxis("Zoom") != 0.00F)
        {
            if(camDistance + (0.2F * Input.GetAxis("Zoom")) > cameraLimits[0] && camDistance + (0.2F * Input.GetAxis("Zoom")) < cameraLimits[1])
                camDistance += 0.2F * Input.GetAxis("Zoom");
        }

        if (Input.GetAxis("CamVertical") != 0.00F && !transPhi)
        {
            //Rotate phi in the positive direction.
            degree = phi * Mathf.Rad2Deg;
            degree += 0.5F * Input.GetAxis("CamVertical");
            phi = degree * Mathf.Deg2Rad;
            angleChangeButtons = true;
            freeCamera = true;
        }
        else if (transPhi && autoAdjustOn)
        {
            if(phi != toPhi && tPhiCount < 50)
            {
                phi += phiInc;
                tPhiCount++;
            }
            else
            {
                transPhi = false;
            }
        }
        if (Input.GetAxis("CamHorizontal") != 0.00F && !transTheta)
        {
            //Rotate theta in the negative direction.
            degree = theta * Mathf.Rad2Deg;
            degree += 0.5F * Input.GetAxis("CamHorizontal");
            theta = degree * Mathf.Deg2Rad;
            angleChangeButtons = true;
            freeCamera = true;
        }
        else if (transTheta && autoAdjustOn)
        {
            if (theta != toTheta && tThetaCount < 50)
            {
                theta += thetaInc;
                tThetaCount++;
            }
            else
            {
                transTheta = false;
            }
        }
        //camDistanceVector = camTransform.position - fairyTransform.position;
        rho = Mathf.Sqrt((camDistance * camDistance) + (camDistance * camDistance) + (camDistance * camDistance));
        angleChangeButtons = false;
        Vector3 camerapos = Vector3.zero;
        camerapos.x = (rho * (Mathf.Sin(phi)) * (Mathf.Cos(theta))) + fairyTransform.position.x;
        camerapos.y = (rho * (Mathf.Cos(phi))) + fairyTransform.position.y;
        camerapos.z = (rho * (Mathf.Sin(phi)) * (Mathf.Sin(theta))) + fairyTransform.position.z;

        camTransform.position = camerapos;
        camTransform.LookAt(fairyTransform.position);
    }
}
