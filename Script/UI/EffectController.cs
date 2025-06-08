using UnityEngine;
using System.Collections.Generic;

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

    [Header("Power Up")]
    [SerializeField] GameObject powerUp;
    [SerializeField] AudioClip powerUpSe;
    [SerializeField] float powerUpSeVol;

    [Header("Character Get")]
    [SerializeField] GameObject characterGet;
    [SerializeField] AudioClip characterGetSe;
    [SerializeField] float characterGetVol;

    [Header("Hit to Player")]
    [SerializeField] GameObject hitPlayer;
    [SerializeField] AudioClip hitPlayerSe;
    [SerializeField] float hitPlayerVol;

    [Header("爆発エフェクト制御設定")]
    [SerializeField] float minimumExplosionDistance = 1.0f;
    [SerializeField] int maxExplosionHistory = 5;

    GameObject playerEffectObj;
    private List<Vector3> oldExplosionPositions = new List<Vector3>();
    

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
            smallExplosionSeVolume, 
            true);
    }
    public void PlaySmallExplosion(Vector3 pos, Quaternion rot, bool isSkipExplosion){
        PlayExplosion(
            pos, 
            rot,
            smallExplosions,
            smallExplosionSes,
            smallExplosionSeVolume, 
            isSkipExplosion);
    }
    public void PlaySmallExplosionSeOnly()
    {
        PlayExplosionSeOnly(smallExplosionSes,
            smallExplosionSeVolume);
    }

    public void PlayMiddleExplosion(Vector3 pos, Quaternion rot)
    {
        PlayExplosion(
            pos,
            rot,
            middleExplosions,
            middleExplosionSes,
            middleExplosionSeVolume, 
            true);
    }
    public void PlayMiddleExplosion(Vector3 pos, Quaternion rot, bool isSkipExplosion)
    {
        PlayExplosion(
            pos,
            rot,
            middleExplosions,
            middleExplosionSes,
            middleExplosionSeVolume, 
            isSkipExplosion);
    }

    public void PlayMiddleExplosionSeOnly()
    {
        PlayExplosionSeOnly(middleExplosionSes,
            middleExplosionSeVolume);
    }

    public void PlayLargeExplosion(Vector3 pos, Quaternion rot)
    {
        PlayExplosion(
            pos,
            rot,
            largeExplosions,
            largeExplosionSes,
            largeExplosionSeVolume, 
            true);
    }
    public void PlayLargeExplosion(Vector3 pos, Quaternion rot, bool isSkipExplosion)
    {
        PlayExplosion(
            pos,
            rot,
            largeExplosions,
            largeExplosionSes,
            largeExplosionSeVolume, 
            isSkipExplosion);
    }
    public void PlayLargeExplosionSeOnly()
    {
        PlayExplosionSeOnly(largeExplosionSes,
            largeExplosionSeVolume);
    }

    void PlayExplosion(Vector3 pos, Quaternion rot, GameObject[] explosions, AudioClip[] clips, float vol, bool isSkipExplosion){
        int objindex = Random.Range(0, explosions.Length);
        int seIndex = Random.Range(0, clips.Length);
        
        if(!isExplosionPositionOK(pos) && isSkipExplosion){
            return;
        }
        
        Instantiate(explosions[objindex], pos, rot);
        SoundManager.Instance.PlaySE(clips[seIndex], vol);
    }

    bool isExplosionPositionOK(Vector3 pos){
        foreach(Vector3 oldPos in oldExplosionPositions){
            if(Vector3.Distance(pos, oldPos) < minimumExplosionDistance){
                return false;
            }
        }
        // 新しい位置を追加
        oldExplosionPositions.Add(pos);
        // 10個を超えたら古いものを削除
        if(oldExplosionPositions.Count > maxExplosionHistory){
            oldExplosionPositions.RemoveAt(0);
        }  
        return true;
    }

    void PlayExplosionSeOnly(AudioClip[] clips, float vol)
    {
        int seIndex = Random.Range(0, clips.Length);
        SoundManager.Instance.PlaySE(clips[seIndex], vol);
    }

    public void PlayPowerUp(Vector3 pos)
    {
        setEffectToPlayer(powerUp, pos, powerUpSe, powerUpSeVol);
    }
    public void PlayCharacterGet(Vector3 pos)
    {
        setEffectToPlayer(characterGet, pos, characterGetSe, characterGetVol);
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
