using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Movement : MonoBehaviour
{
    bool canJump;
    public float jumpSpeed = 5000f;
    private float speed;
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float jumpForce = 50;

    [SerializeField] private CinemachineImpulseSource _impulseSource;

    private Camera _mainCamera;
    private Rigidbody _rb;
    private Controls _controls;
    private Animator _animator;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");       
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int GetPunch = Animator.StringToHash("getPunch");
    private static readonly int GetKick = Animator.StringToHash("getKick");
    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _controls = new Controls();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _controls.Enable();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        _controls.Disable();
    }

    private void Start()
    {
        speed = baseSpeed;
        _mainCamera = Camera.main;
        _rb = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!_controls.Player.Move.IsPressed()) return;
        Vector2 input = _controls.Player.Move.ReadValue<Vector2>();
        Vector3 target = HandleInput(input);
        RotateCharacter(target);
    }

    private void FixedUpdate()
    {
        if (_controls.Player.Punch.IsPressed())
        {
             if (!_animator.GetBool(GetPunch))
            {
                _animator.SetBool(GetPunch, true);
            }
        }
        if (_controls.Player.Kick.IsPressed())
        {
             if (!_animator.GetBool(GetKick))
            {
                _animator.SetBool(GetKick, true);
            }
        }
        if (_controls.Player.Run.IsPressed())
        {
            _animator.SetBool(IsRunning, true);
            _impulseSource.GenerateImpulse();            
            speed = baseSpeed * 1.5f;
        }else{
            _animator.SetBool(IsRunning, false);
            speed = baseSpeed;
        }
        
        if (_controls.Player.Jump.IsPressed() & canJump)
        {
             if (!_animator.GetBool(IsJumping))
            {
                _animator.SetBool(IsJumping, true);
            }
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
   private void RotateCharacter(Vector3 target)
    {
        transform.rotation = Quaternion.LookRotation(target-transform.position);
    }
    private Vector3 HandleInput(Vector2 input)
    {
        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;

        forward.y = 0;
        right.y = 0;
        
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
        if (collision.gameObject.name == "Floor" || collision.gameObject.name == "Container")
        {
            canJump = true;
            _animator.SetBool(IsJumping, false);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Floor" || collision.gameObject.name == "Container")
        {
            canJump = false;
            _animator.SetBool(GetPunch, false);
            _animator.SetBool(GetKick, false);
        }
    }    
}
