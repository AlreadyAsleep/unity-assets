using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CharacterController : MonoBehaviour {

	[System.Serializable]
    public class MoveSettings
    {
        public float moveSpeed = 5f;
        public float dashDistance = 1f;
        
        public float lockRadius = 20;
    }

    [System.Serializable]
    public class LogicSettings
    {
        public static float health = 1000f;
        public static float stamina = 1000f;
        public static float maxHealth = 1000f;
        public static float maxStamina = 1000f;
        public float dashCooldown = 1;
        public float staminaRegenCooldown = 5;
    }

    [System.Serializable]
    public class InputSettings
    {
        //sets keys to move with as defined in Unity Editor
        public string VERTICAL_AXIS = "Vertical";
        public string HORIZONTAL_AXIS = "Horizontal";
        public string DASH_AXIS = "Jump";//spacebar
        public string STRIKE01_AXIS = "Fire1";//Left-Mouse
        public string LOCK_AXIS = "Lock";//right-mouse
        public string BLOCK_AXIS = "Fire2";//q key
    }

    public MoveSettings move = new MoveSettings();
    public InputSettings input = new InputSettings();
    public LogicSettings logic = new LogicSettings();
    Animator animator;

    Vector3 velocity = new Vector3();
    Vector3 direction = Vector3.zero;
    
    
    Rigidbody rbody;
    
    float verticalInput, horizontalInput, dashInput, strike01Input;
    public static float dashTimeStamp;
    public static float staminaTimeStamp;
    public static bool canDash;
    public static bool blocking = false;
    public static bool locked = false;
    public bool attacking = false;
    public bool invincible = false;
    void Start()
    {
        if (GetComponent<Rigidbody>())
        {
            rbody = GetComponent<Rigidbody>();
        }
        else
            Debug.LogError("No RigidBody Defined");//checks if character has rigid body defined
        verticalInput = horizontalInput = dashInput = strike01Input = 0;
        dashTimeStamp = Time.time;
        staminaTimeStamp = Time.time;
        animator = GetComponentInChildren<Animator>();
        
        GameLogic.FindEnemies();
        
        
        
        
    }

    void Update()
    {
        GetInput();
        Move();
        Dash();
        Rotate();
        StartCoroutine(AttackCoroutine());
        Lock();
        Block();
        StaminaRegen();
        rbody.velocity = velocity;

        

        
       
     
        
        
    }

    void GetInput()
    {
        verticalInput = Input.GetAxisRaw(input.VERTICAL_AXIS);
        horizontalInput = Input.GetAxisRaw(input.HORIZONTAL_AXIS);
        dashInput = Input.GetAxisRaw(input.DASH_AXIS);
        strike01Input = Input.GetAxisRaw(input.STRIKE01_AXIS);


    }

    void Move()
    {
        if (!attacking)
        {
            if (!locked)
            {
                //movement animation
                velocity.x = move.moveSpeed * horizontalInput;
                velocity.z = move.moveSpeed * verticalInput;
                velocity.y = 0;
            }
            else
            {
                //movement while locked / strafe animation
                velocity.x = move.moveSpeed * horizontalInput;
                velocity.z = move.moveSpeed * verticalInput;
                velocity.y = 0;
            }
            if (velocity.sqrMagnitude > move.moveSpeed)//prevents player from travelling faster in diagonal
            {
                velocity = velocity.normalized * move.moveSpeed;
            }
        }
        else
        {
            velocity = Vector3.zero;
        }
    }
    

    void Rotate()
    {
        
        if (velocity != Vector3.zero )
        {
            
            direction = velocity;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), .15f);//enables a smooth rotation between directions
        }
        

    }

    void Dash()
    {
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hitPoint;
        bool obstruction = Physics.Raycast(ray, out hitPoint, move.moveSpeed * move.dashDistance + 1f);//prevents player from dashing through colliders
        canDash = Time.time > dashTimeStamp && LogicSettings.stamina >= 100;
        
        

        if ( Input.GetButtonDown(input.DASH_AXIS) && canDash && !obstruction)
        {

            if (locked)
            {       //locked dash animation
                transform.position += new Vector3(horizontalInput, 0, verticalInput).normalized * move.dashDistance * 2;   
            }
            else
            {       //free look dash animation
                transform.position += direction * move.dashDistance;
                animator.Play("Dash", -1, 0f);
            }
            dashTimeStamp = Time.time + logic.dashCooldown;
            staminaTimeStamp = Time.time + logic.staminaRegenCooldown;
            LogicSettings.stamina -= 100;
              
        }
        
    }

    void StaminaRegen()
    {
        bool stamRegen = Time.time > staminaTimeStamp;
        //stamina regen with delay after each use of a stamina depleting event
        if (Environment.TickCount % 5 == 0 && stamRegen && !blocking)
        {
            if (LogicSettings.stamina < LogicSettings.maxStamina)
            {
                LogicSettings.stamina += 1;

                if (LogicSettings.stamina < 0)
                {
                    LogicSettings.stamina = 0;
                }
            }
            else
            {
                LogicSettings.stamina = LogicSettings.maxStamina;
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        //basic attack code and animation
        if(Input.GetButtonDown(input.STRIKE01_AXIS) && !attacking)
        {
            attacking = true;
            animator.Play("Strike001", -1, 0f);
            yield return new WaitForSeconds(.5f);
            attacking = false;
        }
        
      
    }

    

    public Transform FindClosest(List<GameObject> arr)
    {
        Transform target = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject o in arr)
        {
            float dist = Vector3.Distance(o.transform.position, transform.position);
            if (dist < minDist)
            {
                target = o.transform;
                minDist = dist;
            }
        }
        
        return target;
        
        
    }

    void Lock()
    {
        if (FindClosest(GameLogic.enemies).GetComponentInParent<AIController>())
        {
            Transform target = FindClosest(GameLogic.enemies);

            if (Input.GetButtonDown(input.LOCK_AXIS) && Vector3.Distance(transform.position, target.position) < move.lockRadius)
            {
                locked = !locked;
            }


            if (locked )
            {
                transform.LookAt(target);
            }
        }
        if (GameLogic.noEnemies)
        {
            locked = false;
        }

        

    }
    void Block()
    {
        if (Input.GetButton(input.BLOCK_AXIS))
        {
            //blocking code and animation
            animator.Play("Block", -1, 0f);
            blocking = true;
        }
        else
        {
            blocking = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<AIController>())
        {
            AIController enemy;
            if (other.tag == "Weapon")
            {
                enemy = other.gameObject.GetComponentInParent<AIController>();
                if (enemy.attacking && !invincible)
                {

                    StartCoroutine(InvincibleCoroutine());
                    LogicSettings.health -= enemy.logic.damage;

                }

            }
        }
    }
    //adds a brief period of invincibility to avoid multiple strikes within the same attack due to collider errors
    IEnumerator InvincibleCoroutine()
    {
        invincible = true;
        yield return new WaitForSeconds(1f);
        invincible = false;
    }

    private void OnGUI()
    {
        //prints name of enemy locked onto and amount health
        //eventually add a haelth bar instead of value
        if (FindClosest(GameLogic.enemies).GetComponentInParent<AIController>())
        {
            AIController enemy = FindClosest(GameLogic.enemies).GetComponentInParent<AIController>();
            string printEnemy = (locked) ? enemy.tag + ": " + enemy.logic.health : null;
            GUI.Label(new Rect(Screen.width / 2, Screen.height - 20, 100, 40), printEnemy);
        }

    }





}
