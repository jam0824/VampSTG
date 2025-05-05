using UnityEngine;

public class EffectController : MonoBehaviour
{
    // シングルトン化
    public static EffectController Instance { get; private set; }

    [Header("Small Explosion")]
    [SerializeField] GameObject[] smallExplosions;
    [SerializeField] AudioClip[] smallExplosionSes;
    [SerializeField] float smallExplosionSeVolume = 0.5f;

    [Header("Middle Explosion")]
    [SerializeField] GameObject[] middleExplosions;
    [SerializeField] AudioClip[] middleExplosionSes;
    [SerializeField] float middleExplosionSeVolume = 0.5f;

    [Header("Large Explosion")]
    [SerializeField] GameObject[] largeExplosions;
    [SerializeField] AudioClip[] largeExplosionSes;
    [SerializeField] float largeExplosionSeVolume = 0.8f;

    [Header("Effect")]
    [SerializeField] GameObject powerUp;
    [SerializeField] AudioClip powerUpSe;
    [SerializeField] float powerUpSeVol;

    [Header("Hit to Player")]
    [SerializeField] GameObject hitPlayer;
    [SerializeField] AudioClip hitPlayerSe;
    [SerializeField] float hitPlayerVol;

    GameObject playerEffectObj;

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
        PlayExplosion(
            pos, 
            rot,
            smallExplosions,
            smallExplosionSes,
            smallExplosionSeVolume);
    }

    public void PlayMiddleExplosion(Vector3 pos, Quaternion rot){
        PlayExplosion(
            pos, 
            rot,
            middleExplosions,
            middleExplosionSes,
            middleExplosionSeVolume);
    }

    public void PlayLargeExplosion(Vector3 pos, Quaternion rot){
        PlayExplosion(
            pos, 
            rot, 
            largeExplosions, 
            largeExplosionSes, 
            largeExplosionSeVolume);
    }

    void PlayExplosion(Vector3 pos, Quaternion rot, GameObject[] explosions, AudioClip[] clips, float vol){
        int objindex = Random.Range(0, explosions.Length);
        int seIndex = Random.Range(0, clips.Length);
        Instantiate(explosions[objindex], pos, rot);
        SoundManager.Instance.PlaySE(clips[seIndex], vol);
    }

    public void PlayPowerUp(Vector3 pos){
        setEffectToPlayer(powerUp,pos, powerUpSe, powerUpSeVol);
    }

    public void PlayHitToPlayer(Vector3 pos){
        setEffectToPlayer(hitPlayer,pos, hitPlayerSe, hitPlayerVol);
    }

    void setEffectToPlayer(GameObject obj, Vector3 pos, AudioClip clip, float vol){
        GameObject player = getPlayer();
        GameObject effect = Instantiate(obj, pos, Quaternion.identity);
        effect.transform.SetParent(player.transform);
        SoundManager.Instance.PlaySE(clip, vol);
    }

    GameObject getPlayer(){
        if(playerEffectObj == null){
            playerEffectObj = GameObject.Find("PlayerEffectObj");
        }
        return playerEffectObj;
    }
}
