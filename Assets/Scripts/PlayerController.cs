using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 120.0f;
    [SerializeField] private float jumpHeight = 20f;
    [SerializeField] private float gravityValue = -98.1f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float pickupRange = 100;
    [SerializeField] private float moveForce = 250;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform holdParent;

    private Rigidbody CurrentObjectRigidbody;
    private Collider CurrentObjectCollider;
    private Player playerInput;
    private GameObject pickObj;
    private GameObject heldObj;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;
    private Animator anim;
    

    private void Awake()
    {
        playerInput = new Player();
            
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void Start()
    {
        anim=GetComponentInChildren<Animator>();
        cameraTransform = Camera.main.transform;
        controller = gameObject.GetComponent<CharacterController>();
    }

    private void PickupObject(GameObject pickObj)
    {
        if (pickObj.GetComponent<Rigidbody>())
        {
            Rigidbody objRig = pickObj.GetComponent<Rigidbody>();
            objRig.useGravity = false;
            objRig.drag = 10;

            objRig.transform.parent = holdParent;
            heldObj = pickObj;
        }
    }

    private void MoveObject()
    {
        if (Vector3.Distance(heldObj.transform.position, holdParent.position) > 0.1f)
        {
            Vector3 moveDirection = (holdParent.position - heldObj.transform.position);
            heldObj.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);
         }
    }

    private void DropObject()
    {
        Rigidbody heldRig = heldObj.GetComponent<Rigidbody>();
        heldRig.useGravity = true;
        heldRig.drag = 0;
        heldObj.transform.parent = null;
        heldObj = null; 
    }


    void Update()
    {
        groundedPlayer = controller.isGrounded;
        
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            anim.SetFloat("Speed", 0 ,0.1f, Time.deltaTime);
        }

        if (playerInput.PlayerMain.Pickup.triggered)
        {

            /*Ray Pickupray = new Ray(holdParent.transform.position, PlayerCamera.transform.forward);
            {
                if (Physics.Raycast(Pickupray, out RaycastHit hitInfo, pickupRange, pickupLayer))
                {
                    if (CurrentObjectRigidbody) { }
                    else 
                    {
                        CurrentObjectRigidbody = hitInfo.rigidbody;
                        CurrentObjectCollider = hitInfo.collider;
              
                        CurrentObjectRigidbodyisKinematic = true;
                        CurrentObjectCollider.enabled = false;
                    }
                }
            }*/
            if (heldObj == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, pickupLayer))
                {
                    PickupObject(hit.transform.gameObject);
                }
            }

            else 
            {
                DropObject();
            }
        }

        if (heldObj != null)
        {
            MoveObject();
        }

            Vector2 movementInput = playerInput.PlayerMain.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(movementInput.x, 0f, movementInput.y);
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

       
        if (playerInput.PlayerMain.Jump.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (movementInput != Vector2.zero)
        {
            if (groundedPlayer) { anim.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime); }
            else { anim.SetFloat("Speed", 0f, 0.1f, Time.deltaTime); }
        float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        }
    }
}