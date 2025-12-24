using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    public bool isClimbing { get; private set; } = false ; // Состояние, когда паук лазит по стене
    private Vector2 wallNormal;  // Нормаль стены, чтобы определить ее направление
    private float detachCooldown = 0.2f; // Время задержки после отцепления
    private float lastDetachTime = -1f; // Время последнего отцепления
    private bool inputLocked = false; // Блокировка управления
    private float horizontalInput;
    public bool isWallSliding { get; private set; } = false;
    private float wallSlidingSpeed = 2f;
    private float slideCooldown = 0.5f; // Время задержки после скольжения
    private float lastSlideTime;
    private float waitedTime = 3f;
    private float lastWaitTime;
    private bool isLevitate = false;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (inputLocked)
            return; // Игнорируем ввод, если он заблокирован

        if (isClimbing)
        {
            //transform.rotation = Quaternion.Euler(0, 0, 90);
            if (!onWall() && !(lastWaitTime > waitedTime) && !isLevitate)
            {
                body.linearVelocity = Vector2.zero; // Останавливаем движение
                lastWaitTime += Time.deltaTime;
                return;
            }
            // Движение по стене
            body.gravityScale = 0; // Отключаем гравитацию
            body.linearVelocity = new Vector2(0, verticalInput * speed);

            // Включаем флаг, чтобы указать, что можно выполнять поворот
            anim.SetBool("climb", false);

            if (wallNormal.x > 0) // Стена справа
            {
                // Разворот паука в зависимости от направления
                if (verticalInput > 0.01f)
                {
                    transform.localScale = new Vector3(1, -1, 1); // Смотрит вверх
                    transform.eulerAngles = new Vector3(0, 0, 90);
                }
                else if (verticalInput < -0.01f)
                {
                    transform.localScale = new Vector3(-1, -1, 1); // Смотрит вниз
                    transform.eulerAngles = new Vector3(0, 0, 90);
                }
            }
            else if (wallNormal.x < 0) // Стена слева
            {
                // Разворот паука в зависимости от направления
                if (verticalInput > 0.01f)
                {
                    transform.localScale = new Vector3(1, 1, 1); // Смотрит вверх
                    transform.eulerAngles = new Vector3(0, 0, 90);
                }
                else if (verticalInput < -0.01f)
                {
                    transform.localScale = new Vector3(-1, 1, 1); // Смотрит вниз
                    transform.eulerAngles = new Vector3(0, 0, 90);
                }
            }


            // Отцепиться от стены
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R) || lastWaitTime > waitedTime || isLevitate)
            {
                outFromWall();
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SoundManager.instance.PlaySound(jumpSound);
                }
            }

        }
        else
        {
            // Обычное движение

            transform.rotation = Quaternion.Euler(0, 0, 0);
            // Разворот паука на земле
            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
            if (isGrounded() || (!isGrounded() && !onWall()))
            {
                body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
                if (Input.GetKeyDown(KeyCode.Space) && (isGrounded() || (onWall() && !isGrounded() && horizontalInput != 0 && Mathf.Sign(-wallNormal.x) != Mathf.Sign(horizontalInput))))
                {
                    SoundManager.instance.PlaySound(jumpSound);
                }
            }   

            WallSlide();
        }
        anim.SetBool("run", isClimbing ? verticalInput != 0 : horizontalInput != 0);
        anim.SetBool("grounded", isGrounded());


        // Прилипание к стене
        if (onWall() && Input.GetKeyDown(KeyCode.R) && Time.time - lastDetachTime > detachCooldown && wallNormal != Vector2.zero)
        {
            isClimbing = true;
            inputLocked = true; // Блокируем управление
            body.gravityScale = 0; // Отключаем гравитацию
            body.linearVelocity = Vector2.zero; // Останавливаем движение
            anim.SetTrigger("stick"); // Анимация прилипания
            StartCoroutine(WaitForAnimation());
        }
    }
    private void Jump()
    {
        if (isGrounded())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
            anim.SetTrigger("jump");
        }
        else if (onWall() && !isGrounded())
        {
            if (horizontalInput != 0)
            {
                // Определяем направление стены: если "смотрим" на стену
                float wallDirection = Mathf.Sign(-wallNormal.x); // 1, если смотрим вправо, -1, если влево
                float playerDirection = Mathf.Sign(horizontalInput);

                // Прыжок в сторону движения
                if (playerDirection != wallDirection)
                {
                    body.linearVelocity = new Vector2(horizontalInput * speed, jumpPower);
                    anim.SetTrigger("jump");
                }
            }
        }
    }
    public void OnStickAnimationComplete()
    {
        // Получаем размер коллайдера
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            // Сдвигаем паука к стене с учетом нормали
            Vector2 offset = wallNormal * collider.bounds.extents.x;
            transform.position -= (Vector3)offset; // Двигаем паука ближе к стене
        }

        // Настройка поворота и масштаба в зависимости от нормали стены
        if (wallNormal.x > 0) // Стена справа
        {
            transform.localScale = new Vector3(1, -1, 1); // Смотрит вверх
            transform.eulerAngles = new Vector3(0, 0, 90);
        }
        else if (wallNormal.x < 0) // Стена слева
        {
            // Разворот паука в зависимости от направления
            transform.localScale = new Vector3(1, 1, 1); // Смотрит вверх
            transform.eulerAngles = new Vector3(0, 0, 90);
        }
        Bounds bounds = boxCollider.bounds;
        // Проверяем наличие стены сверху
        RaycastHit2D leftTopHit = Physics2D.Raycast(new Vector2(bounds.min.x, bounds.max.y), Vector2.left, 0.1f, wallLayer);

        // Проверяем наличие стены снизу
        RaycastHit2D leftBottomHit = Physics2D.Raycast(new Vector2(bounds.min.x, bounds.min.y), Vector2.left, 0.1f, wallLayer);

        // Проверяем наличие стены сверху
        RaycastHit2D rightTopHit = Physics2D.Raycast(new Vector2(bounds.max.x, bounds.max.y), Vector2.right, 0.1f, wallLayer);

        // Проверяем наличие стены снизу
        RaycastHit2D rightBottomHit = Physics2D.Raycast(new Vector2(bounds.max.x, bounds.min.y), Vector2.right, 0.1f, wallLayer);

        // Проверяем, есть ли перед пауком стена
        Vector2 wallDirection = wallNormal != Vector2.zero ? -wallNormal : Vector2.right * transform.localScale.x;
        RaycastHit2D wallHit = Physics2D.BoxCast(bounds.center, bounds.size, 0, wallDirection, 0.1f, wallLayer);
        if (wallHit.collider != null && wallNormal != Vector2.zero)
        {
            // Проверяем движение вверх
            if (transform.localScale.y == 1 && rightTopHit.collider == null && rightBottomHit.collider == null)
            {
                isLevitate = true; // Верхняя граница достигнута
            }

            // Проверяем движение вниз
            if (transform.localScale.y == -1 && leftTopHit.collider == null && leftBottomHit.collider == null)
            {
                isLevitate = true; // Нижняя граница достигнута
            }
        }
        else
        {
            isLevitate = true; // Стены нет
        }
        inputLocked = false; // Разблокируем управление
    }

    private IEnumerator WaitForAnimation()
    {
        if (isGrounded())
        {
            float animationTime = anim.GetCurrentAnimatorStateInfo(0).length / 3;
            yield return new WaitForSeconds(animationTime);
        }
        else
        {
            float animationTime = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationTime);
        }
        OnStickAnimationComplete();
    }

    public bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    public bool onWall()
    {
        // Получаем размеры и границы коллайдера паука
        Bounds bounds = boxCollider.bounds;

        // Верхняя и нижняя точки проверки
        Vector2 topCheck = new Vector2(bounds.min.x, bounds.max.y);  // Верхний край
        Vector2 bottomCheck = new Vector2(bounds.min.x, bounds.min.y);  // Нижний край
        Vector2 rightTopCheck = new Vector2(bounds.max.x, bounds.max.y);  // Верхний край
        Vector2 rightBottomCheck = new Vector2(bounds.max.x, bounds.min.y);  // Нижний край

        // Проверяем наличие стены сверху
        RaycastHit2D leftTopHit = Physics2D.Raycast(new Vector2(bounds.min.x, bounds.max.y), Vector2.left, 0.1f, wallLayer);

        // Проверяем наличие стены снизу
        RaycastHit2D leftBottomHit = Physics2D.Raycast(new Vector2(bounds.min.x, bounds.min.y), Vector2.left, 0.1f, wallLayer);

        // Проверяем наличие стены сверху
        RaycastHit2D rightTopHit = Physics2D.Raycast(new Vector2(bounds.max.x, bounds.max.y), Vector2.right, 0.1f, wallLayer);

        // Проверяем наличие стены снизу
        RaycastHit2D rightBottomHit = Physics2D.Raycast(new Vector2(bounds.max.x, bounds.min.y), Vector2.right, 0.1f, wallLayer);

        // Визуализация для отладки
        Debug.DrawRay(topCheck, Vector2.left * 0.1f, Color.red);
        Debug.DrawRay(bottomCheck, Vector2.left * 0.1f, Color.blue);
        Debug.DrawRay(rightTopCheck, Vector2.right * 0.1f, Color.green);
        Debug.DrawRay(rightBottomCheck, Vector2.right * 0.1f, Color.magenta);

        // Проверяем, есть ли перед пауком стена
        Vector2 wallDirection = wallNormal != Vector2.zero ? -wallNormal : Vector2.right * transform.localScale.x;
        RaycastHit2D wallHit = Physics2D.BoxCast(bounds.center, bounds.size, 0, wallDirection, 0.1f, wallLayer);

        if (wallHit.collider != null)
        {
            // Проверяем движение вверх
            if (Input.GetAxis("Vertical") > 0 && leftTopHit.collider == null && rightTopHit.collider == null)
            {
                return false; // Верхняя граница достигнута
            }

            // Проверяем движение вниз
            if (Input.GetAxis("Vertical") < 0 && leftBottomHit.collider == null && rightBottomHit.collider == null)
            {
                return false; // Нижняя граница достигнута
            }

            return true; // Можно двигаться
        }

        return false; // Стены нет
    }

    public void outFromWall()
    {
        isLevitate = false;
        isClimbing = false;
        lastWaitTime = 0;
        body.gravityScale = 2;

        body.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.y), 10);

        // Возвращаем паука в изначальное положение
        transform.eulerAngles = Vector3.zero;
        transform.localScale = new Vector3(transform.localScale.y, 1, 1);

        anim.SetTrigger("jump");
        body.linearVelocity = new Vector2(horizontalInput * speed, 10);

        lastDetachTime = Time.time; // Устанавливаем время отцепления
    }
    private void WallSlide()
    {
        if (onWall() && !isGrounded() && horizontalInput != 0f)
        {
            // Определяем направление стены: если "смотрим" на стену
            float wallDirection = Mathf.Sign(-wallNormal.x); // 1, если смотрим вправо, -1, если влево
            float playerDirection = Mathf.Sign(horizontalInput);
            // Проверяем, движется ли паук в сторону стены
            if (playerDirection == wallDirection)
            {
                isWallSliding = true;
                body.linearVelocity = new Vector2(body.linearVelocity.x, Mathf.Clamp(body.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
            }
            else
            {
                if (isWallSliding)
                {
                    lastSlideTime = Time.time;
                }

                if (Time.time - lastSlideTime > slideCooldown)
                {
                    body.linearVelocity = new Vector2(horizontalInput * speed, 2);
                }
                // Отключаем скольжение, если пользователь изменил направление
                isWallSliding = false;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Получаем нормаль стены из первой точки контакта
            wallNormal = collision.contacts[0].normal;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            wallNormal = Vector2.zero; // Сбрасываем нормаль
        }
    }

    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !isClimbing;
    }
}
