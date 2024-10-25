using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private CharacterController _characterController;

    //capa de obstaculos
    [SerializeField] private LayerMask _obstacleLayer; // Capa de obstáculos

    // configuraciones de movimientoz
    [SerializeField] private float _NORMAL_SPEED = 4f;
    [SerializeField] private float _RUN_SPEED = 6f;
    [SerializeField] private float _GRAVITY = 9.81f;
    [SerializeField] private float _OBSTACLE_CHECK_DISTANCE = 1.5f; // Distancia para comprobar obstáculos
    private int _runtineMovement;
    private Quaternion _lookRotation;
    private float _grade;
    private float _cronometer;
    private Vector3 _moveDirection; // Vector para la dirección del movimiento


    // Rango de visión del enemigo
    [SerializeField] private float _visionRange = 10f; // Rango de visión
    [SerializeField] private float _visionAngle = 180f; // Ángulo del cono de visión

    // estados
    private bool _playerDetected = false;
    private Transform _playerTransform;


    public Animator moster;


    // Start is called before the first frame update
    void Start()
    {
        InitializeEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlayer(); // Detectar al jugador
        ApplyGravity();

        // Si el jugador es detectado, el enemigo se mueve hacia el jugador
        if (_playerDetected)
        {
            MoveTowardsPlayer();
        }
        else
        {
            normalMovement();
        }

    }

    private void InitializeEnemy()
    {
        _characterController = GetComponent<CharacterController>();
        _playerTransform = GameObject.Find("Player").transform; // Buscar el objeto Player y obtener su transform
    }

    private void normalMovement()
    {
        // rutina de movimiento y descanzo aleatorio
        _cronometer += 1 * Time.deltaTime;

        if (_cronometer >= 3)
        {
            _runtineMovement = Random.Range(0, 2); // 0 = no se mueve, 1 = se mueve
            _cronometer = 0;
        }

        switch (_runtineMovement)
        {
            case 0:
                moster.SetTrigger("NoCamina");
                Debug.Log("NO está caminando");
                break;
            case 1:
                Debug.Log("SI está caminando");
                moster.SetTrigger("Camina");
                _grade = Random.Range(0, 360);
                _lookRotation = Quaternion.Euler(0, _grade, 0);
                _runtineMovement = 2;
                
                break;
            case 2:
                Debug.Log("Está en un obstaculo");
                // Comprobar si hay un obstáculo frente al enemigo
                if (IsObstacleInFront())
                {
                    // Cambiar dirección 45 grados a la derecha
                    _grade += 45f;
                    _lookRotation = Quaternion.Euler(0, _grade, 0);
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, 0.1f);
                _characterController.Move(transform.forward * _NORMAL_SPEED * Time.deltaTime);               
                break;
        }
    }

    // Método para comprobar si hay un obstáculo en frente
    private bool IsObstacleInFront()
    {
        RaycastHit hit;
        // Realiza un Raycast en la dirección hacia adelante del enemigo
        if (Physics.Raycast(transform.position, transform.forward, out hit, _OBSTACLE_CHECK_DISTANCE, _obstacleLayer))
        {
            return true; // Hay un obstáculo en frente
        }
        return false; // No hay obstáculos
    }

    // TODO: Implementar la paqueteria de IA para que esquibe por si sola las paredes y en caso de perder al jugador lo busque en la ultima posicion conocida por 7 segundos

    // detectar al jugador
    private void DetectPlayer()
    {
        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized; // Dirección hacia el jugador

        // Comprobar si el jugador está dentro del rango de visión y del ángulo
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (distanceToPlayer <= _visionRange && angleToPlayer < _visionAngle / 2)
        {
            // Comprobar si hay algún obstáculo entre el enemigo y el jugador
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, _obstacleLayer))
            {
                // No hay obstáculos, jugador detectado
                _playerDetected = true;
            }
            else
            {
                _playerDetected = false;
            }
        }
        else
        {
            _playerDetected = false;
        }
    }

    // Método para dibujar el rango de visión del enemigo en el editor
    private void MoveTowardsPlayer()
    {
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        _characterController.Move(direction * _RUN_SPEED * Time.deltaTime);
        transform.LookAt(new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z)); // Enfocar hacia el jugador
    }

    private void ApplyGravity()
    {
        if (!_characterController.isGrounded)
        {
            // Aplicamos la gravedad en el eje Y (hacia abajo)
            _moveDirection.y -= _GRAVITY * Time.deltaTime;
        }
        else
        {
            // Si está en el suelo, reiniciamos la fuerza de caída
            _moveDirection.y = -1f; // Mantenerlo "pegado" al suelo, con una leve fuerza hacia abajo
        }

        // Aplicamos el movimiento (incluyendo el componente de gravedad)
        _characterController.Move(_moveDirection * Time.deltaTime);
    }


}
