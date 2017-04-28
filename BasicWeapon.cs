using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeapon : MonoBehaviour {

    [System.Serializable]
    public class StatSettings
    {
        public float baseDamage = 50f;
        public bool hasModifier = false;
    }

    public GameObject owner;
    [HideInInspector]
    public CharacterController charController;
    [HideInInspector]
    public AIController aiController;

    public StatSettings stats = new StatSettings();
    
    void Start()
    {
        SetOwner(owner);
    }

    void Update()
    {
        
    }
    //basic code for detecting attacks 
    void OnTriggerEnter(Collider other)
    {
        
  
    }

    void SetOwner(GameObject o)
    {
        owner = o;

        if (owner.GetComponent<CharacterController>())
        {
            charController = owner.GetComponent<CharacterController>();
        }

        if (owner.GetComponent<AIController>())
        {
            aiController = owner.GetComponent<AIController>();
           
        }

    }

    
}
