using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    [Header("Player Movement Properties")]
    [SerializeField] private float playerWalkSpeed = 3f;
    [SerializeField] private float playerRunSpeed = 6f;
    [SerializeField] private float jumpingPower = 16f;
    private float horizontalMovementInput;
    private float verticalMovementInput;


    [Tooltip("increasing will enable player to jump even after not grounded")]
    [SerializeField] private float coyoteTime = 0.2f; //increasing will enable player to jump even after not grounded
    private float coyoteTimeCounter;

    [Tooltip("increasing will add buffer to the jump")]
    [SerializeField] private float jumpBufferTime = 0.2f;//increasing will add buffer to the jump 
    private float jumpBufferCounter;

    private float shotAnimationDelay = 1.0f;
    private float attack1AnimationDelay = 0.600f;
    private float attack2AnimationDelay = 0.300f;

    private float rechargeAnimationDelay = 1.500f;

    [Header("Player Animator")]
    [SerializeField] private Animator playerAnimator;
    private string currentState;

    //Animations States

    const string PLAYER_IDLE = "Idle";
    const string PLAYER_WALK = "Walk";
    const string PLAYER_RUN = "Run";

    const string PLAYER_JUMP = "Jump";
    const string PLAYER_FALL = "Fall";
    const string PLAYER_LAND = "Land";

    const string PLAYER_ATTACK1 = "Attack1";
    const string PLAYER_ATTACK2 = "Attack2";
    const string PLAYER_SHOT = "Shot";
    const string PLAYER_RECHARGE = "Recharge";

    const string PLAYER_HURT = "Hurt";
    const string PLAYER_DEATH = "Death";


    [Header("Camera Tracking")]
    [SerializeField] private GameObject _cameraFollowGO;
    private CameraFollowObject _cameraFollowObject;
    public CinemachineVirtualCamera virtualCamera;
    float currentOrthoSize;
    float normalOrtho = 3.6f;
    float runningOrtho = 5.5f;


    //flags
    public bool IsRunning = false;
    private bool death = false;
    public bool isFacingRight = true;
    private bool isJumping;
    private bool jumpCooldown;
  
    //for shot
    private bool IsShotFiring;
    private bool IsShotPressed;

    //for attack1
    private bool IsAttacking1;
    private bool IsAttack1Pressed;

    //for attack2
    private bool IsAttacking2;
    private bool IsAttack2Pressed;

    //for recharge
    private bool IsRecharging;
    private bool IsRechargePressed;

    [Header("Ground Checks")]
    [SerializeField] private Transform leftGroundCheck;
    [SerializeField] private Transform rightGroundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rigidbody2d;

    private Gamepad gamepad;
    
    private bool canRun;
    private GameObject pumpkinPortal; // Reference to the Pumpkin portal GameObject


    private void Start() {
        currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        currentOrthoSize = normalOrtho;
        Application.targetFrameRate = 60;// limiting in-game FPS to 60
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//this will make sure that our 

        Cursor.visible = false;
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
        pumpkinPortal = GameObject.Find("Pumpkin Portal"); // Find the Pumpkin portal GameObject by name

        // Get the first connected gamepad
        if (Gamepad.all.Count > 0) {
            gamepad = Gamepad.all[0];
        }
    }

    void Update() {
        //Debug.Log("Current ortho size: " + currentOrthoSize);
        if (IsGrounded()) {
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !jumpCooldown) {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, jumpingPower);
            jumpBufferCounter = 0f;
            ChangeAnimationState(PLAYER_JUMP);

            StartCoroutine(JumpCooldown());

            if (!isJumping) {
                rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, rigidbody2d.velocity.y * 0.5f);
                ChangeAnimationState(PLAYER_JUMP);
            }
        } else {
            jumpBufferCounter -= Time.deltaTime;
        }        
    }

    private void FixedUpdate() {

        if ( !IsShotFiring ) {
            rigidbody2d.velocity = new Vector2(horizontalMovementInput * playerWalkSpeed, rigidbody2d.velocity.y);
        }

        if (!IsShotFiring && IsRunning && canRun) {
            rigidbody2d.velocity = new Vector2(horizontalMovementInput * playerRunSpeed, rigidbody2d.velocity.y);
        }

        if (horizontalMovementInput > 0f || horizontalMovementInput < 0f) {
            TurnCheck();
        }

        if (pumpkinPortal != null) {
            float distanceToPortal = Vector3.Distance(transform.position, pumpkinPortal.transform.position);
            if (distanceToPortal < 4f) {
                canRun = false; // Disable running if player is too close to the portal             
            } else {
                canRun = true; // Enable running if player is far enough from the portal
            }
        }

        // Update orthographic size of the virtual camera based on whether the player is running or not
        if (IsRunning) {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, runningOrtho, Time.fixedDeltaTime * 2f); // Adjust the interpolation speed as needed
        } else {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, normalOrtho, Time.fixedDeltaTime * 2f); // Adjust the interpolation speed as needed
        }
    }

    #region Input Actions Events

    //Event for Move Input
    public void Move(InputAction.CallbackContext context) {
        horizontalMovementInput = context.ReadValue<Vector2>().x;
        verticalMovementInput = context.ReadValue<Vector2>().y;
    }

    //Event for Running Input
    public void RunningStarted(InputAction.CallbackContext context) {
        if(context.performed) {
            IsRunning = true;
        }
    }

    public void RunningFinished(InputAction.CallbackContext context) {
        if (context.performed) {
            IsRunning = false;
        }
    }

    //Event for Jump Input
    public void Jump(InputAction.CallbackContext context) {
        if (context.performed) {
            isJumping = true;
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled) {
            isJumping = false;

            if (rigidbody2d.velocity.y > 0f) {
                rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, rigidbody2d.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
    }

    //Event for Shot Input
    public void Shot(InputAction.CallbackContext context) {
        if (context.performed) {
            IsShotPressed = true;
            
        }
    }

    //Event for Attack1 Input
    public void Attack1(InputAction.CallbackContext context) {
        if (context.performed) {
            IsAttack1Pressed = true;
            
        }
    }

    //Event for Attack2 Input
    public void Attack2(InputAction.CallbackContext context) {
        if (context.performed) {
            IsAttack2Pressed = true;
            
        }
    }

    //Event for Recharge Input
    public void Recharge(InputAction.CallbackContext context) {
        if (context.performed) {
            IsRechargePressed = true;//change this for Recharge
            
        }
    }

    //Event for Interact Event

    public void Interact(InputAction.CallbackContext context) {
        if (context.performed) {
            Debug.Log("Interact");
        }
    }
    #endregion

    public bool IsGrounded() {
        Vector2 lGroundCheckPoint = leftGroundCheck.TransformPoint(Vector3.zero);
        Vector2 rGroundCheckPoint = rightGroundCheck.TransformPoint(Vector3.zero);

        return Physics2D.OverlapArea(lGroundCheckPoint, rGroundCheckPoint, groundLayer);
    }

    private IEnumerator JumpCooldown() {
        jumpCooldown = true;
        yield return new WaitForSeconds(0.4f);
        jumpCooldown = false;
    }

    private void TurnCheck() {
        if (horizontalMovementInput > 0f && !isFacingRight) {
            Turn();
        } else if (horizontalMovementInput < 0f && isFacingRight) {
            Turn();
        }
    }

    private void Turn() {
        if (isFacingRight) {

            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            _cameraFollowObject.CallTurn();
        } else {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            _cameraFollowObject.CallTurn();
        }
    }

    #region Player Animations

    void ChangeAnimationState(string newState) {

        //stop the same animation from interrupting itself
        if (currentState == newState) return;

        //play the animation
        playerAnimator.Play(newState);
        //Debug.Log(newState);

        //reassign the current state
        currentState = newState;
    }

    private void LateUpdate() {
        var velocity = rigidbody2d.velocity;
        var yVelocity = velocity.y;

        

        if (IsGrounded() && NotAttacking() && !isJumping) {
            if (velocity.x != 0f && !IsRunning) {
                //currentOrthoSize = normalOrtho;
                ChangeAnimationState(PLAYER_WALK);
            } else if (velocity.x != 0f && IsRunning) {
                //currentOrthoSize = runningOrtho;
                ChangeAnimationState(PLAYER_RUN);
            } else {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }

         bool NotAttacking() {
            if (!IsShotFiring && !IsShotPressed && !IsAttacking1 && !IsAttacking2 && !IsAttack1Pressed && !IsAttack2Pressed && !IsRecharging && !IsRechargePressed) {
                return true;
            }else { return false; }
         }

        if (IsShotPressed && !IsRechargePressed && !IsRecharging) {
            
            IsShotPressed = false;

            if (!IsShotFiring) {
                IsShotFiring = true;
                // Freeze rotation along Z-axis to prevent movement along X-axis
                FreezeX();

                if (IsGrounded()) {
                    // Trigger rumble with intensity 0.5 for 0.2 seconds
                    gamepad.SetMotorSpeeds(1f, 1f);
                    // Stop rumble after 0.2 seconds
                    StartCoroutine(StopRumble(.24f));
                    ChangeAnimationState(PLAYER_SHOT);
                }
                Invoke("ShotFiringComplete", shotAnimationDelay);
            }
        }

        if (IsAttack1Pressed ) {
            IsAttack1Pressed = false;

            if (!IsAttacking1) {
                IsAttacking1 = true;
                // Freeze rotation along Z-axis to prevent movement along X-axis
                FreezeX();
                if (IsGrounded()) {
                    ChangeAnimationState(PLAYER_ATTACK1);
                    
                }
                Invoke("Attack1Complete", attack1AnimationDelay);
            }
        }

        if (IsAttack2Pressed) {
            IsAttack2Pressed = false;

            if (!IsAttacking2) {
                IsAttacking2 = true;
                // Freeze rotation along Z-axis to prevent movement along X-axis
                FreezeX();

                if (IsGrounded()) {
                    ChangeAnimationState(PLAYER_ATTACK2);
                }
                Invoke("Attack2Complete", attack2AnimationDelay);
            }
        }

        if (IsRechargePressed && !IsShotPressed && !IsShotFiring) {
            IsRechargePressed = false;

            if (!IsRecharging) {
                IsRecharging = true;
                // Freeze rotation along Z-axis to prevent movement along X-axis
                FreezeX();

                if (IsGrounded()) {
                    ChangeAnimationState(PLAYER_RECHARGE);
                }
                Invoke("RechargeComplete",rechargeAnimationDelay);
            }
        }

        if (yVelocity < 0f && !IsGrounded()) {

            ChangeAnimationState(PLAYER_FALL);
            isJumping = false;
        }

        if (death) {
            ChangeAnimationState(PLAYER_DEATH);

            Invoke("ResetLevel", 1f);
        }
    }

    void ShotFiringComplete() {
        IsShotFiring = false;
        // Reset Rigidbody constraints after shot animation completes
        UnFreezeX();
        
    }

    void Attack1Complete() {
        IsAttacking1 = false;
        // Reset Rigidbody constraints after attack1 animation completes
        UnFreezeX();
    }

    void Attack2Complete() {
        IsAttacking2 = false;
        // Reset Rigidbody constraints after attack2 animation completes
        UnFreezeX();
    }
    void RechargeComplete() {
        IsRecharging = false;
        // Reset Rigidbody constraints after attack2 animation completes
        UnFreezeX();
    }


    #endregion

    void ResetLevel() {
        Debug.Log("Reset level");
        SceneManager.LoadScene("Gameplay");
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        // Trigger rumble with intensity 0.5 for 0.2 seconds
        gamepad.SetMotorSpeeds(0.5f, 0.9f);
        // Stop rumble after 0.2 seconds
        StartCoroutine(StopRumble(0.2f));
    }
    private void Die() {
        death = true;
    }

    void FreezeX() {
        rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void UnFreezeX() {
        rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private IEnumerator StopRumble(float duration) {
        yield return new WaitForSeconds(duration);
        // Stop rumble
        gamepad.SetMotorSpeeds(0, 0);
    }

    
}