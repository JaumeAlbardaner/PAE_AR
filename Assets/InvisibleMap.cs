using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       // get all renderers in this object and its children:
        var renders = GetComponentsInChildren<Renderer>();
        foreach (var rendr in renders){
        rendr.material.renderQueue = 2002; // set their renderQueue
     }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
