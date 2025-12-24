using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance {  get; private set; }
    private AudioSource sourse;

    private void Awake()
    {
        sourse = GetComponent<AudioSource>();

        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //Destroy duplicate gameObject
        else if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void PlaySound(AudioClip _sound)
    {
        sourse.PlayOneShot(_sound);
    }
}
