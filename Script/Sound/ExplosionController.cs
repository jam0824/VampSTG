using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    // シングルトン化
    public static ExplosionController Instance { get; private set; }

    [Header("Small Explosion")]
    [SerializeField] GameObject[] smallExplosions;
    [SerializeField] AudioClip smallExplosionSe;
    [SerializeField] float smallExplosionSeVolume = 0.5f;

    private void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySmallExplosion(Vector3 pos, Quaternion rot){
        int index = Random.Range(0, smallExplosions.Length);
        Instantiate(smallExplosions[index], pos, rot);
        SoundManager.Instance.PlaySE(smallExplosionSe, smallExplosionSeVolume);
    }
}
