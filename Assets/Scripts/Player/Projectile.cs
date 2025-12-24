using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    private float direction;
    private bool hit;
    private BoxCollider2D boxCollider;
    private Animator anim;
    [SerializeField] private AudioClip spalshSound;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();   
    }

    private void Update()
    {
        if (hit)
            return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        hit = true;
        SoundManager.instance.PlaySound(spalshSound);
        boxCollider.enabled = false;
        anim.SetTrigger("explode");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        SoundManager.instance.PlaySound(spalshSound);
        anim.SetTrigger("explode");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.GetComponent<Health>().TakeDamage(1);
        }
    }
    public void SetDirection(float _direction)
    {
        direction = _direction;
        hit = false;
        boxCollider.enabled = true;

        if(transform.localScale.x != _direction)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    private void Deactivate()
    {
        Destroy(gameObject);
    }
}
