using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIController : MonoBehaviour {

    [System.Serializable]
    public class MoveSettings
    {
        public float moveSpeed = 2f;
    }

    [System.Serializable]
    public class AISettings
    {
        public float attackFrequency = 1f;
        public float detectionRadius = 20f;
        public float attack01Radius = 2f;
    }

    [System.Serializable]
    public class LogicSettigs
    {
        public float health = 200f;
        public float stamina = 400f;
        public float damage = 150f;
    }

    public static float attack01Timestamp;
    [HideInInspector]
    public  bool attacking = false;
    [HideInInspector]
    public bool invincible = false;
    public MoveSettings move = new MoveSettings();
    public AISettings AI = new AISettings();
    public LogicSettigs logic = new LogicSettigs();
    CharacterController charController;
    Rigidbody rbody;
    GameObject obj;
    Vector3 velocity = new Vector3();
    Vector3 direction = Vector3.zero;
    public Transform target;
    public Animator animator;
    
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        obj = GetComponent<GameObject>();
        SetTarget(target);
    }

    void Update()
    {

        
        Move();
        Detect();
        StartCoroutine(AttackCoroutine());
        Rotate();
        Die();

    }

   

    void SetTarget(Transform t)
    {
        target = t;

        if (target != null)
        {
            if (target.GetComponent<CharacterController>())
            {
                charController = target.GetComponent<CharacterController>();
            }
            else
                Debug.LogError("Need Character Contoller");
        }
        else
            Debug.LogError("no target");
    }

    void Follow()
    {
        if (!attacking)
        {
            if (Detect())
            {
                velocity = transform.forward * move.moveSpeed;
            }
            else
            {
                velocity = Vector3.zero;
            }
            velocity.y = 0;
        }
        else
        {
            velocity = Vector3.zero;
            
        }
    }

    bool Detect()
    {
        float distance;
        float difX = target.position.x - transform.position.x;
        float difZ = target.position.z - transform.position.z;
        distance = Mathf.Sqrt(Mathf.Pow(difX, 2) + Mathf.Pow(difZ, 2));//Euclidian Distance formula
        if (distance < AI.detectionRadius)
        {
            return true;
        }

        return false;
    }  
            
    

    void Move()
    {
       
            Follow();
            rbody.velocity = velocity;   
    }

    IEnumerator AttackCoroutine()
    {
        bool close = false;
        float distance;
        float difX = target.position.x - transform.position.x;
        float difZ = target.position.z - transform.position.z;
        distance = Mathf.Sqrt(Mathf.Pow(difX, 2) + Mathf.Pow(difZ, 2));//Euclidian Distance formula
        if (distance < AI.attack01Radius)
        {
            close = true;
        }
        else
        {
            close = false;
        }
        if (close)
        {   
            if(Time.time >= attack01Timestamp)//2 second cooldown on basic attack
            {
                attacking = true;
                animator.Play("Strike001", -1, 0f);
                attack01Timestamp = Time.time + 2;
                yield return new WaitForSeconds(1f);
                attacking = false;
            }
        }

    }

    void Rotate()
    {
        if (Detect() && !attacking)
        {
            transform.LookAt(target);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BasicWeapon weapon;
        if(other.tag == "Weapon")
        {
            weapon = other.gameObject.GetComponent<BasicWeapon>();
            if (weapon.owner.GetComponentInChildren<CharacterController>().attacking && !invincible)
            { 
                //Play take damage animation
                logic.health -= weapon.stats.baseDamage;
                StartCoroutine(InvincibleCoroutine());    
            }
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        invincible = true;
        yield return new WaitForSeconds(.5f);
        invincible = false;
    }

    void Die()
    {
        if(logic.health <= 0)
        {
            //Play Death animation
            GameLogic.enemies.Remove(GetComponent<GameObject>());
            gameObject.SetActive(false);
            /* foreach(GameObject o in transform.GetComponentsInChildren<GameObject>())
            {
                Destroy(o);
            }*/
            // Destroy(this);
            GameLogic.killCount++;
        }
    }
}
