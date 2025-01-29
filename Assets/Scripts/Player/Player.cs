//using UnityEngine;

//public class Player : MonoBehaviour
//{
//    // Public Properties
//    public Rigidbody Rigidbody { get; private set; }
//    public Animator Animator { get; private set; }

//    [Header("Movement Parameters")]
//    [SerializeField] private float walkSpeed = 5f;
//    [SerializeField] private float runSpeed = 10f;
//    [SerializeField] private float jumpForce = 10f;

//    private Vector3 moveDirection;
//    private bool isRunning;
//    private bool isJumping;
//    private Vector2 lookInput;

//    private void Awake()
//    {
//        InitializeComponents();
//    }

//    private void Update()
//    {
//        HandleMovement();
//        HandleJump();
//        HandleLook();
//        HandleAttack();
//    }

//    #region Initialization
//    private void InitializeComponents()
//    {
//        Rigidbody = GetComponent<Rigidbody>() ?? throw new System.NullReferenceException("Rigidbody component missing!");
//        Animator = GetComponent<Animator>() ?? throw new System.NullReferenceException("Animator component missing!");
//    }
//    #endregion

//    #region Movement Handling
//    private void HandleMovement()
//    {
//        moveDirection = InputSystemManager.Instance.MoveInput;

//        if (moveDirection.magnitude > 0)
//        {
//            float speed = InputSystemManager.Instance.RunInput > 0 ? runSpeed : walkSpeed;
//            Vector3 movement = new Vector3(moveDirection.x, 0, moveDirection.z) * speed * Time.deltaTime;
//            Rigidbody.MovePosition(Rigidbody.position + movement);

//            // Rotate player to face movement direction
//            if (moveDirection != Vector3.zero)
//            {
//                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
//                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
//            }

//            Animator.SetBool("isMoving", true);
//        }
//        else
//        {
//            //Animator.SetBool("isMoving", false);
//        }
//    }
//    #endregion

//    #region Jump Handling
//    private void HandleJump()
//    {
//        if (InputSystemManager.Instance.JumpInput && IsGrounded())
//        {
//            Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
//            Animator.SetTrigger("Jump");
//        }
//    }

//    private bool IsGrounded()
//    {
//        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
//    }
//    #endregion

//    #region Look Handling
//    private void HandleLook()
//    {
//        lookInput = InputSystemManager.Instance.LookInput;
//        if (lookInput.magnitude > 0)
//        {
//            float rotationSpeed = 100f;
//            float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
//            transform.Rotate(Vector3.up, yaw);
//        }
//    }
//    #endregion

//    #region Attack Handling
//    private void HandleAttack()
//    {
//        if (InputSystemManager.Instance.AttackInput)
//        {
//           Animator.SetTrigger("Attack");
//        }
//    }
//    #endregion

//    #region Gizmos for Debugging
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = IsGrounded() ? Color.green : Color.red;
//        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 1.1f);
//    }
//    #endregion
//}
