using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallCube : MonoBehaviour
{
    //referense till kubens renderer, så gridManagern ska kunna accessa shadern
    public SpriteRenderer rend;
    
    
    
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    
   
}
