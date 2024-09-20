using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase principal que controla las acciones del jugador
public class PlayerController : MonoBehaviour
{
    // Referencias
    private CharacterController _characterController;
    private Transform _cameraTransform;

    // configuraciones de movimiento
    [SerializeField] private float _NORMAL_SPEED = 5f;
    [SerializeField] private float _RUN_SPEED = 13f;
    [SerializeField] private float _CROUNCH_SPEED = 2f;
    [SerializeField] private float _GRAVITY = 9.81f;
    [SerializeField] private float _JUMP_FORCE = 2.5f;
    private float _moveSpeed;
    [SerializeField] private float _fallVelocity;

    // Estados del jugador
    private bool _isCrouching;

    // Constantes para el movimiento, rotacion y altura
    private float _ROTATION_SPEED = 8f;
    private float _CROUNCH_HEIGHT = 0.75f;
    private float _NORMAL_HEIGHT = 1.5f;
    private float _RAYCAST_DISTANCE_STANDING = 1.25f;

    private Vector3 _moveDirection;
    private Vector3 _playerInputAxis;


    void Awake()
    {
        InitializePlayer();
    }

    void Start()
    {
        InitializePlayer();
    }

    void Update()
    {
        PlayerMovement();
        HandleMouseRotation();
        JumpHandler();
        RunHandler();
        CrouchingHandler();

        _characterController.Move(_moveDirection * _moveSpeed * Time.deltaTime);
    }


    // inicializacion de jugador
    private void InitializePlayer()
    {
        _characterController = GetComponent<CharacterController>(); // Obtenemos el componente CharacterController
        _cameraTransform = transform.Find("Camera"); // Obtenemos la camara del jugador
        Cursor.lockState = CursorLockMode.Locked; // Bloqueamos el cursor
    }

    // Configuramos el movimiento del jugador
    private void PlayerMovement()
    {
        // Obtener la entrada del movimiento del jugador
        _playerInputAxis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Normalizamos el vector de movimiento para que el jugador no se mueva mas rapido en diagonal
        _playerInputAxis = _playerInputAxis.magnitude > 1 // Si la magnitud del vector es mayor a 1
            ? transform.TransformDirection(_playerInputAxis).normalized 
            : transform.TransformDirection(_playerInputAxis);

        // asignamos la direccion de movimiento a las variables de movimiento
        _moveDirection.x = _playerInputAxis.x;
        _moveDirection.z = _playerInputAxis.z;
    }

    // rotacion del mause
    private void HandleMouseRotation()
    {
        // Rotar al jugador según el movimiento del ratón en X y Z //
        transform.Rotate(0, Input.GetAxis("Mouse X") * _ROTATION_SPEED, 0);


        // Rotar la cámara del jugador según el movimiento del ratón en Y //
        float desiredRotationX = _cameraTransform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 8;

        // Ajuste del ángulo para que esté entre -180 y 180 grados (manejo de rotación en Unity)
        if (desiredRotationX > 180) desiredRotationX -= 360;
        if (desiredRotationX < -180) desiredRotationX += 360;

        // Solo permite la rotación si el ángulo está dentro del rango permitido (-180 a 180 grados)
        if (desiredRotationX >= -180 && desiredRotationX <= 180)
        {
            // Se aplica la rotación calculada a la cámara
            _cameraTransform.localRotation = Quaternion.Euler(desiredRotationX, 0, 0);
        }
    }

    // salto del jugador
    private void JumpHandler()
    {
        if (_characterController.isGrounded)
        {
            _fallVelocity = -1f; // Resetea la velocidad de caída si está en el suelo
            if (Input.GetKeyDown(KeyCode.Space) && !_isCrouching)
            {
                 _fallVelocity = _JUMP_FORCE; // Aplica la fuerza de salto
            }
        }
        else
        {
            // Aplica la gravedad si está en el aire
            _fallVelocity -= _GRAVITY * Time.deltaTime;
        }
        _moveDirection.y = _fallVelocity;
    }

    // acelearcion del jugador
    private void RunHandler()
    {
        // Ajustar la velocidad dependiendo de si está corriendo o no
        if (Input.GetKey(KeyCode.LeftShift) && !_isCrouching)
        {
            _moveSpeed = _RUN_SPEED;
        }
        else if (!_isCrouching)
        {
            _moveSpeed = _NORMAL_SPEED;
        }
    }

    // agacharse del jugador
    private void CrouchingHandler()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_isCrouching)
            {
                RaycastHit hit;

                if (!Physics.Raycast(transform.position, Vector3.up, out hit, _RAYCAST_DISTANCE_STANDING))
                {
                    _isCrouching = false;
                    _characterController.height = _NORMAL_HEIGHT;
                    _moveSpeed = _NORMAL_SPEED;
                }
            }
            else
            {
                _isCrouching = true;
                _characterController.height = _CROUNCH_HEIGHT;
                _moveSpeed = _CROUNCH_SPEED;
            }
        }
    }
}
