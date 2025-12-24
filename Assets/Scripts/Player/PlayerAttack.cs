using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда
    [SerializeField] private Transform firePoint; // Точка вылета снаряда
    private Animator anim;
    private PlayerMovement playerMovement;
    private float cooldownTimer = 100;
    [SerializeField] private AudioClip poisonballSound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if(Input.GetMouseButton(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            Attack();
        }
        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        SoundManager.instance.PlaySound(poisonballSound);
        anim.SetTrigger("attack");
        cooldownTimer = 0;
        
        StartCoroutine(WaitForAnimation());
    }
    private IEnumerator WaitForAnimation()
    {
        float animationTime = anim.GetCurrentAnimatorStateInfo(0).length * 2f;
        yield return new WaitForSeconds(animationTime);

        if (playerMovement.isGrounded() && !playerMovement.isClimbing)
        {
            // Создание снаряда
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Установка направления
            float direction = transform.localScale.x > 0 ? 1 : -1; // Зависит от направления персонажа
            projectile.GetComponent<Projectile>().SetDirection(direction);
        }
    }
}
