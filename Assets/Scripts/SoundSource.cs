using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour {

    GameObject _player;
    public AudioClip[] sounds;
    AudioSource myAudio;
    AudioClip currentClip;

    public GameObject idle, singing;

    public float soundTimerTotal;
    float soundTimer;

    bool playing;

	void Start () {
        myAudio = GetComponent<AudioSource>();

        _player = GameObject.FindGameObjectWithTag("Player");

        float randomTime = Random.Range(-2, 2);

        soundTimerTotal += randomTime;

        RandomizeClip();

        singing.SetActive(false);

        soundTimer = soundTimerTotal;
	}
	
	void Update () {
		if(Vector3.Distance(transform.position, _player.transform.position) < 50)
        {
            playing = true;
            
        }
        else
        {
            playing = false;
        }

        if (playing)
        {
            soundTimer -= Time.deltaTime;
            if(soundTimer < 0)
            {
                PlaySound();
            }
        }

        if (myAudio.isPlaying)
        {
            idle.SetActive(false);
            singing.SetActive(true);
        }
        else
        {
            idle.SetActive(true);
            singing.SetActive(false);
        }
	}

    void RandomizeClip()
    {
        int randomSound = Random.Range(0, sounds.Length);

        currentClip = sounds[randomSound];
    }

    void PlaySound()
    {
        myAudio.PlayOneShot(currentClip);
        soundTimer = soundTimerTotal;

        int randomSwitch = Random.Range(0, 100);
        if (randomSwitch > 50)
        {
            RandomizeClip();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && !myAudio.isPlaying)
        {
            PlaySound();
        }
    }
}
