using UnityEngine;

public class EffectController : MonoBehaviour
{
    // シングルトン化
    public static EffectController Instance { get; private set; }

    [Header("Small Explosion")]
    [SerializeField] GameObject[] smallExplosions;
    [SerializeField] AudioClip[] smallExplosionSes;
    [SerializeField] float smallExplosionSeVolume = 0.5f;

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
        int objindex = Random.Range(0, smallExplosions.Length);
        int seIndex = Random.Range(0, smallExplosionSes.Length);
        Instantiate(smallExplosions[objindex], pos, rot);
        SoundManager.Instance.PlaySE(smallExplosionSes[seIndex], smallExplosionSeVolume);
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
