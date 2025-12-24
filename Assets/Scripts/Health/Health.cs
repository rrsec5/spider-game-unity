using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth {  get; private set; }
    private Animator anim;
    public bool dead { get; private set; }
    private PlayerMovement playerMovement;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] public int numberOfFlashes;
    private SpriteRenderer spriteRend;

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
        if(currentHealth > 0)
        {
            //player hurt
            anim.SetTrigger("hurt");
            //iframes
            StartCoroutine(Invunerability(false));
            SoundManager.instance.PlaySound(hurtSound);
        }
        else
        {
            //player dead
            if(!dead)
            {
                anim.SetTrigger("die");
                dead = true;
                //Player
                if (GetComponent<PlayerMovement>() != null)
                {
                    if (!playerMovement.isClimbing)
                        GetComponent<PlayerMovement>().enabled = false;
                    else
                    {
                        playerMovement.outFromWall();
                        GetComponent<PlayerMovement>().enabled = false;
                    }
                    StartCoroutine(Invunerability(true));
                }

                //Enemy
                if(GetComponentInParent<EnemyPatrol>() != null)
                GetComponentInParent<EnemyPatrol>().enabled = false;

                if (GetComponentInParent<MeleeEnemy>() != null)
                    GetComponent<MeleeEnemy>().enabled = false;
                SoundManager.instance.PlaySound(deathSound);
            }
        }
    }
    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
    private IEnumerator Invunerability(bool _dead)
    {
        Physics2D.IgnoreLayerCollision(10, 7, true);
        Physics2D.IgnoreLayerCollision(10, 11, true);
        //invunerabity duration
        if (!_dead)
        {
            for (int i = 0; i < numberOfFlashes; i++)
            {
                spriteRend.color = new Color(1, 0, 0, 0.5f);
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
                spriteRend.color = Color.white;
                yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            }
        }
        else
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
        }
        Physics2D.IgnoreLayerCollision(10, 7, false);
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
    public void RespawnHealth()
    {
        dead = false;
        AddHealth(startingHealth);
        anim.ResetTrigger("die");
        anim.Play("Idle");
        if (GetComponent<PlayerMovement>() != null)
            GetComponent<PlayerMovement>().enabled = true;
    }
}
