﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ThirdPersonController : MonoBehaviour
{
    //Player movment variables. 
    CharacterController player;
    public GameObject currentPartner;
    public float currentSpeed, walkSpeed, runSpeedMax;
    public float startingHeight, runTime;
    private Vector3 targetPosition;
    public bool isMoving, isClimax; // for point to click
    
    float clickTimer;
    public GameObject walkingPointer;
    //Camera ref variables
    AudioSource cameraAudSource;
    public AudioSource playerSource;
    public AudioClip[] footsteps;
    public float walkStepTotal = 1f, runStepTotal = 0.5f;
    float footStepTimer = 0;
    int currentStep = 0;
    CameraController camControl;

    //set publicly to tell this script what raycasts can and can't go thru
    public LayerMask mask;

    //dictionary to sort nearby audio sources by distance 
    Dictionary<AudioSource, float> soundCreators = new Dictionary<AudioSource, float>();

    //listener range
    public float listeningRadius;
    //store this mouse pos
    Vector3 lastPosition;

    public GameObject idle, walking, sex, fight;

    //UI walking
    Image symbol; // 2d sprite renderer icon reference
    AnimateUI symbolAnimator;
    List<Sprite> walkingSprites = new List<Sprite>(); // walking feet cursor
    int currentWalk = 0;

    float climaxDelay =0;

    void Start()
    {
        //walking UI
        symbol = GameObject.FindGameObjectWithTag("Symbol").GetComponent<Image>(); //searches for InteractSymbol
        symbolAnimator = symbol.GetComponent<AnimateUI>();
        for (int i = 1; i < 4; i++)
        {
            walkingSprites.Add(Resources.Load<Sprite>("CursorSprites/Foot" + i));
        }

        symbol.sprite = walkingSprites[currentWalk];
        playerSource = GetComponent<AudioSource>();

        //cam refs
        cameraAudSource = Camera.main.GetComponent<AudioSource>();
        camControl = Camera.main.GetComponent<CameraController>();

        //set starting points for most vars
        player = GetComponent<CharacterController>();
        targetPosition = transform.position;

        walking.SetActive(false);
        sex.SetActive(false);
        fight.SetActive(false);

        startingHeight = transform.position.y;
        currentSpeed = walkSpeed;

    }

    void Update()
    {
        if (!isClimax)
        {
            //click to move to point
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                clickTimer += Time.deltaTime;
                if (clickTimer > runTime && currentSpeed < runSpeedMax)
                {
                    currentSpeed += Time.deltaTime * 5;
                }

                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    //if we hit the ground & height is in range, move the character to that position
                    if (hit.transform.gameObject.tag == "Ground")
                    {
                        walkingPointer.transform.position = hit.point;
                        targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                        isMoving = true;

                    }

                    //if we hit an interactable object AND we are far from it, the player should auto walk towards it
                    else if (Vector3.Distance(transform.position, hit.transform.position) > 5 &&
                        (hit.transform.gameObject.tag == "Animal"))
                    {
                       
                            targetPosition = new Vector3(hit.point.x + 2, transform.position.y, hit.point.z + 2);
                            walkingPointer.transform.position = new Vector3(targetPosition.x, targetPosition.y - 1, targetPosition.z);
                            isMoving = true;
                        
                        

                    }
                    else
                    {
                        isMoving = false;
                    }
                }
            }

            //On mouse up, we check clickTimer to see if we are walking to that point or stopping the character from running 
            if (Input.GetMouseButtonUp(0))
            {
                playerSource.PlayOneShot(footsteps[currentStep]);
                //increment footstep audio
                if (currentStep < (footsteps.Length - 1))
                {
                    currentStep++;
                }
                else
                {
                    currentStep = 0;
                }
                if (clickTimer < runTime)
                {
                    isMoving = true;
                    clickTimer = 0;
                    currentSpeed = walkSpeed;
                    //set walk sprite
                    if (currentWalk < (walkingSprites.Count - 1))
                    {
                        currentWalk++;
                    }
                    else
                    {
                        currentWalk = 0;
                    }
                    symbol.sprite = walkingSprites[currentWalk];
                    symbolAnimator.active = false;
                    walkingPointer.SetActive(true);
                }
                else
                {
                    symbolAnimator.active = false;
                    isMoving = false;
                    clickTimer = 0;
                    currentSpeed = walkSpeed;
                }
            }

            //Check if we are moving and transition animation controller
            if (isMoving)
            {

                MovePlayer();
                idle.SetActive(false);
                sex.SetActive(false);
                fight.SetActive(false);
                walking.SetActive(true);

                footStepTimer += Time.deltaTime;
                
                if (currentSpeed > 12)
                {
                    //play footstep sound
                    if (footStepTimer > runStepTotal)
                    {
                        playerSource.PlayOneShot(footsteps[currentStep]);
                        //increment footstep audio
                        if (currentStep < (footsteps.Length - 1))
                        {
                            currentStep+= Random.Range(0, (footsteps.Length - currentStep));
                        }
                        else
                        {
                            currentStep = 0;
                        }
                        footStepTimer = 0;
                    }
                    //animate ui
                    walkingPointer.SetActive(false);
                    symbolAnimator.active = true;
                }
                else
                {
                    //play footstep sound
                    if (footStepTimer > walkStepTotal)
                    {
                        playerSource.PlayOneShot(footsteps[currentStep]);
                        //increment footstep audio
                        if (currentStep < (footsteps.Length - 1))
                        {
                            currentStep += Random.Range(0, (footsteps.Length - currentStep));
                        }
                        else
                        {
                            currentStep = 0;
                        }
                        footStepTimer = 0;
                    }
                }

              
            }
            //this timer only plays the idle animation if we are not moving. still a little buggy
            else
            {
              
                footStepTimer = 0;
                walkingPointer.SetActive(false);

                idle.SetActive(true);
                sex.SetActive(false);
                fight.SetActive(false);
                walking.SetActive(false);
            }

            //if mouse has moved, refill list & reevaluate priorities
            if (lastPosition != transform.position)
            {
                ResetNearbyAudioSources();
            }

            lastPosition = transform.position;
        }
        else
        {
            climaxDelay += Time.deltaTime;

            //transform.RotateAround(currentPartner.transform.position, Vector3.up, 50 * Time.deltaTime );

            if((currentPartner.GetComponent<Animal>().animalState != Animal.AnimalState.SEXY 
                && currentPartner.GetComponent<Animal>().animalState != Animal.AnimalState.FIGHTING) && climaxDelay > 1)
            {
                isClimax = false;
                idle.SetActive(true);
                walking.SetActive(false);
                sex.SetActive(false);
                fight.SetActive(false);
                climaxDelay = 0;
            }
        }
    }
   
    //Movement function which relies on vector3 movetowards. when we arrive at target, stop moving.
    void MovePlayer()
    {
        
        //first calculate rotation and look
        targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        float currentDist = Vector3.Distance(transform.position, targetPosition);

        if(targetPosition.x < transform.position.x)
        {
            idle.GetComponent<SpriteRenderer>().flipX = false;
            walking.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            idle.GetComponent<SpriteRenderer>().flipX = true;
            walking.GetComponent<SpriteRenderer>().flipX = true;
        }

        //this is a bit finnicky with char controller so may need to continuously set it 
        if (currentDist >= 0.5f)
        {
            transform.LookAt(targetPosition);

            //then set movement
            Vector3 movement = new Vector3(0, 0, currentSpeed);

            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);

            movement = transform.rotation * movement;

            //Actually move
            player.Move(movement * Time.deltaTime);

            player.Move(new Vector3(0, -0.5f, 0));
        }
        else
        {
            isMoving = false;
           
        }

       
    }

    //this function shifts all audio source priorities dynamically
    void ResetNearbyAudioSources()
    {
        //empty dictionary
        soundCreators.Clear();
        //overlap sphere to find nearby sound creators
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, listeningRadius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            //check to see if obj is plant or rock
            if (hitColliders[i].gameObject.tag == "Plant" || hitColliders[i].gameObject.tag == "Rock" || 
                hitColliders[i].gameObject.tag == "NPC" || hitColliders[i].gameObject.tag == "RainSplash" 
                || hitColliders[i].gameObject.tag == "Ambient" || hitColliders[i].gameObject.tag == "Animal" 
                || hitColliders[i].gameObject.tag == "Seed")
            {
                //check distance and add to list
                float distanceAway = Vector3.Distance(hitColliders[i].transform.position, transform.position);
                //add to audiosource and distance to dictionary
                soundCreators.Add(hitColliders[i].gameObject.GetComponent<AudioSource>(), distanceAway);


            }
            i++;
        }

        int priority = 0;
        //sort the dictionary by order of ascending distance away
        foreach (KeyValuePair<AudioSource, float> item in soundCreators.OrderBy(key => key.Value))
        {
            // do something with item.Key and item.Value
            item.Key.priority = priority;
            priority++;
        }
    }
}
