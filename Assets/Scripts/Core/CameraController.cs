using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float cameraSpeed;
    [SerializeField] private float cameraHigh;
    private float lookAhead;
    private PlayerMovement playerMovement;

    private Vector3 targetPosition; // Целевая позиция камеры

    private void Awake()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        transform.position = new Vector3(player.position.x + lookAhead, player.position.y + cameraHigh, transform.position.z);
        if (playerMovement.isClimbing)
        {
            lookAhead = Mathf.Lerp(lookAhead, -(aheadDistance * player.localScale.y), Time.deltaTime * cameraSpeed);
        }
        else if (playerMovement.onWall() && !playerMovement.isGrounded())
        {
            lookAhead = Mathf.Lerp(lookAhead, -(aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);
        }
        else
        {
            lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);
        }
        
    }
}
