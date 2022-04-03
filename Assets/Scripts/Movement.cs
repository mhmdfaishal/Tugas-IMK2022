using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    public Vector2 move;
    public Vector3 look;
    public float jumpSpeed = 5000f;
    bool canJump;
    [SerializeField] private int speed = 10;
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private float jumpForce = 50;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private float cameraRotationLimit = 85f;
    [SerializeField] private float Sensitivity;

    private Camera _mainCamera;
    private Rigidbody _rb;
    private Controls _controls;
    private Animator _animator;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");       

    private void Awake()
    {
        _controls = new Controls();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _controls.Enable();

        _controls.Player.Move.performed += ctx => {
            move = ctx.ReadValue<Vector2>();
        };

        _controls.Player.Move.canceled += ctx => {
            move = Vector2.zero;
        };

        _controls.Player.Look.performed += ctx => {
            look = ctx.ReadValue<Vector3>();
        };

        _controls.Player.Look.canceled += ctx => {
            look = Vector3.zero;
        };
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        _controls.Disable();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _rb = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (usePhysics)
        {
            return;
        }
        if (_controls.Player.Pause.IsPressed())
        {
            SceneManager.LoadScene("SampleScene");
        }
        
        if (_controls.Player.Jump.IsPressed() & canJump)
        {   
            _rb.AddForce(0f, jumpSpeed * Time.deltaTime, 0f);
        }
        
        if (_controls.Player.Move.IsPressed())
        {
            _animator.SetBool(IsWalking, true);
            Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
            Vector3 target = HandleInput(input);
            Move(target);
        }
        else
        {
            _animator.SetBool(IsWalking, false);
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause(){
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MenuScene");
    }

    private void FixedUpdate()
    {
        if (!usePhysics)
        {
            return;
        }
        
        if (_controls.Player.Pause.IsPressed())
        {
            Pause();
        }
        if (_controls.Player.Resume.IsPressed())
        {
            Resume();
        }

        if (_controls.Player.Run.IsPressed())
        {
            CameraShake.Instance.Shaking(0.1f, 0.1f);
            speed = 50;
        }else{
            speed = 10;
        }
        
        if (_controls.Player.Jump.IsPressed() & canJump)
        {
            _rb.AddForce(1f, jumpSpeed * Time.deltaTime, 2f);
        }

        if (_controls.Player.Move.IsPressed())
        {
            _animator.SetBool(IsWalking, true);
            
            Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
            Vector3 target = HandleInput(input);
            MovePhysics(target);
        }
        else
        {
            _animator.SetBool(IsWalking, false);
        }
    }

    private Vector3 HandleInput(Vector2 input)
    {
        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;

        forward.y = 0;
        right.y = 1;
        
        forward.Normalize();
        right.Normalize();

        Vector3 direction = right * input.x + forward * input.y;
        
        return transform.position + direction * speed * Time.deltaTime;
    }

    private void Move(Vector3 target)
    {
        transform.position = target;
    }

    private void MovePhysics(Vector3 target)
    {
        _rb.MovePosition(target); 
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            canJump = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            canJump = false;
        }
    }    
}
