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

    Vector3 origPos, targetPos;

    public float moveRadius, moveSpeed, idleTimerTotal, sexyTimeTotal, fightTimeTotal;
    float idleTimer, sexyTimer, fightTimer;

    public enum AnimalState
    {
        WALKING, SEXY, FIGHTING,
    }
    
	void Start () {

        _player = GameObject.FindGameObjectWithTag("Player");
        tpc = _player.GetComponent<ThirdPersonController>();
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();

        walking.SetActive(false);
        sex.SetActive(false);
        fight.SetActive(false);

        origPos = transform.position;
        FindNewPoint();
        animalState = AnimalState.WALKING;

        idleTimer = idleTimerTotal;
        sexyTimer = sexyTimeTotal;
        fightTimer = fightTimeTotal;
    }
	
	void Update () {
		if(animalState == AnimalState.WALKING)
        {
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
            walking.SetActive(false);
            idle.SetActive(false);
            sex.SetActive(true);
            fight.SetActive(false);

            sexyTimer -= Time.deltaTime;
            if(sexyTimer < 0)
            {
                StartCoroutine(EndClimax());
            }
        }
        else if (animalState == AnimalState.FIGHTING)
        {
            walking.SetActive(false);
            idle.SetActive(false);
            sex.SetActive(false);
            fight.SetActive(true);

            fightTimer -= Time.deltaTime;
            if(fightTimer < 0)
            {
                StartCoroutine(EndClimax());
            }
        }
    }

    void FindNewPoint()
    {
        Vector2 xy = Random.insideUnitCircle * moveRadius;

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
            currentPartner = other.gameObject;
            int randomSex = Random.Range(0, 100);
            if(randomSex < 50)
            {
                sexyTimer = sexyTimeTotal;
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
                fightTimer = fightTimeTotal;
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
            int randomSex = Random.Range(0, 100);
            if (randomSex < 50)
            {
                sexyTimer = sexyTimeTotal;
                animalState = AnimalState.SEXY;
                other.GetComponent<Animal>().animalState = AnimalState.SEXY;
            }
            else
            {
                fightTimer = fightTimeTotal;
                animalState = AnimalState.FIGHTING;
                other.GetComponent<Animal>().animalState = AnimalState.FIGHTING;
            }
        }
    }
    
}
