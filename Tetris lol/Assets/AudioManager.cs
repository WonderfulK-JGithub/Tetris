using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip bopClip;
    public AudioClip rotateClip;
    public AudioClip lineClearClip;
    public AudioClip errorClip;

    [HideInInspector]
    public AudioSource source;

    
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

   


    
}
