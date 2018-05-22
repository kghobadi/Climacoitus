using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour {

    Rigidbody rb;
    BoxCollider myCollider;

    GameObject _player;
    ThirdPersonController tpc;

    GameObject currentPartner;

    public GameObject idle, walking, sex, fight;

    public AnimalState animalState;

    public bool alpha;

    Vector3 origPos, targetPos;

    public float moveRadius, moveSpeed, idleTimerTotal, climaxTotal;
    float idleTimer, sexyTimer, fightTimer;

    AudioSource myVoice;
    public AudioClip[] sexy, fighting;
    AudioClip sexxer, fighter;

    public enum AnimalState
    {
        WALKING, SEXY, FIGHTING, DEAD
    }
    
	void Start () {

        _player = GameObject.FindGameObjectWithTag("Player");
        tpc = _player.GetComponent<ThirdPersonController>();
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();

        myVoice = GetComponent<AudioSource>();

        walking.SetActive(false);
        sex.SetActive(false);
        fight.SetActive(false);

        origPos = transform.position;
        FindNewPoint();
        animalState = AnimalState.WALKING;

        idleTimer = idleTimerTotal;

        float randomClimax = Random.Range(-climaxTotal / 2, climaxTotal / 2);

        climaxTotal += randomClimax;

        int RandomSex = Random.Range(0, sexy.Length);
        int RandomFight = Random.Range(0, fighting.Length);

        sexxer = sexy[RandomSex];
        fighter = fighting[RandomFight];
    }
	
	void Update () {
		if(animalState == AnimalState.WALKING)
        {
            myVoice.Stop();

            if(targetPos.x < transform.position.x)
            {
                idle.GetComponent<SpriteRenderer>().flipX = false;
                walking.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                idle.GetComponent<SpriteRenderer>().flipX = true;
                walking.GetComponent<SpriteRenderer>().flipX = true;
            }

            //walk to point
            if(Vector3.Distance(transform.position, targetPos) > 1)
            {
                walking.SetActive(true);
                idle.SetActive(false);
                sex.SetActive(false);
                fight.SetActive(false);

                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            //idle
            else
            {
                walking.SetActive(false);
                idle.SetActive(true);
                sex.SetActive(false);
                fight.SetActive(false);

                //idle then find new point
                idleTimer -= Time.deltaTime;
                if(idleTimer < 0)
                {
                    FindNewPoint();
                    idleTimer = idleTimerTotal;
                }
            }
        }
        else if (animalState == AnimalState.SEXY)
        {
            if (!myVoice.isPlaying)
            {
                myVoice.clip = sexxer;
                myVoice.Play();
            }

            walking.SetActive(false);
            idle.SetActive(false);
            sex.SetActive(true);
            fight.SetActive(false);

            sexyTimer -= Time.deltaTime;
            if(sexyTimer < 0 && alpha)
            {
                StartCoroutine(EndClimax());
            }
        }
        else if (animalState == AnimalState.FIGHTING)
        {
            if (!myVoice.isPlaying)
            {
                myVoice.clip = fighter;
                myVoice.Play();
            }

            walking.SetActive(false);
            idle.SetActive(false);
            sex.SetActive(false);
            fight.SetActive(true);

            fightTimer -= Time.deltaTime;
            if(fightTimer < 0 && alpha)
            {
                StartCoroutine(EndClimax());
            }
        }
    }

    void FindNewPoint()
    {
        Vector2 myPos = new Vector2(transform.position.x, transform.position.z);

        Vector2 xy = myPos + (Random.insideUnitCircle * moveRadius);

        targetPos = new Vector3(xy.x, origPos.y, xy.y);
    }

    IEnumerator EndClimax()
    {
        rb.isKinematic = false;
        myCollider.isTrigger = false;

        float randomForceX = Random.Range(15, 30);

        float randomForceZ = Random.Range(15, 30);

        rb.AddForce(randomForceX, 0, randomForceZ);

        yield return new WaitForSeconds(1);

        animalState = AnimalState.WALKING;
        rb.isKinematic = true;
        myCollider.isTrigger = true;
        alpha = false;

        if (currentPartner != null && currentPartner != _player)
        {
            currentPartner.GetComponent<Animal>().animalState = AnimalState.WALKING;
        }

        if(currentPartner == _player)
        {
            tpc.isClimax = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            alpha = true;
            currentPartner = other.gameObject;
            int randomSex = Random.Range(0, 100);
            if(randomSex < 50)
            {
                sexyTimer = climaxTotal;
                animalState = AnimalState.SEXY;

                tpc.isClimax = true;
                tpc.currentPartner = gameObject;
                tpc.sex.SetActive(true);
                tpc.idle.SetActive(false);
                tpc.walking.SetActive(false);
                tpc.fight.SetActive(false);
            }
            else
            {
                fightTimer = climaxTotal;
                animalState = AnimalState.FIGHTING;

                tpc.isClimax = true;
                tpc.currentPartner = gameObject;
                tpc.sex.SetActive(false);
                tpc.idle.SetActive(false);
                tpc.walking.SetActive(false);
                tpc.fight.SetActive(true);
            }
        }
        else if(other.gameObject.tag == "Animal")
        {
            currentPartner = other.gameObject;
            if(currentPartner.GetComponent<Animal>().climaxTotal > climaxTotal)
            {
                currentPartner.GetComponent<Animal>().alpha = true;
            }
            else
            {
                alpha = true;
            }
            int randomSex = Random.Range(0, 100);
            if (randomSex < 50)
            {
                sexyTimer = climaxTotal;
                animalState = AnimalState.SEXY;
                other.GetComponent<Animal>().animalState = AnimalState.SEXY;
            }
            else
            {
                fightTimer = climaxTotal;
                animalState = AnimalState.FIGHTING;
                other.GetComponent<Animal>().animalState = AnimalState.FIGHTING;
            }
        }
    }
    
}
