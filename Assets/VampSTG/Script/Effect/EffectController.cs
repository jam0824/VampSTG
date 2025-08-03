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

    [Header("カメラ設定")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera effectCamera;
    

    GameObject playerEffectObj;
    private List<Vector3> oldExplosionPositions = new List<Vector3>();

    private GameObject _effectPool;
    private GameObject _playerBulletPool;
    private GameObject _enemyBulletPool;
    

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

    void Start()
    {
        _effectPool = transform.Find("EffectPool").gameObject;
        _playerBulletPool = transform.Find("PlayerBulletPool").gameObject;
        _enemyBulletPool = transform.Find("EnemyBulletPool").gameObject;
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
        
        GameObject explosion = PlayEffect(explosions[objindex], pos, rot);
        if (_effectPool != null)
        {
            explosion.transform.SetParent(_effectPool.transform);
        }
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

    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public GameObject PlayEffect(GameObject obj, Vector3 pos, Quaternion rot){
        Vector3 correctedPos = pos;
        
        // オブジェクトのレイヤーがEffectの場合のみ座標変換
        if (obj.layer == LayerMask.NameToLayer("Effect"))
        {
            if (effectCamera != null && mainCamera != null)
            {
                correctedPos = GetCorrectedPosition(mainCamera, effectCamera, pos);
            }
            else
            {
                Debug.LogWarning("effectCameraまたはmainCameraが設定されていません");
            }
        }
        
        GameObject effect = Instantiate(obj, correctedPos, rot);
        if (_effectPool != null)
        {
            effect.transform.SetParent(_effectPool.transform);
        }
        return effect;
    }

    /// <summary>
    /// プレイヤーの弾を再生する（オブジェクトプーリング対応）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public GameObject PlayPlayerBullet(GameObject obj, Vector3 pos, Quaternion rot){   
        GameObject bullet = null;
        
        // _playerBulletPoolから同じ名前の非アクティブなオブジェクトを探す
        if (_playerBulletPool != null)
        {
            bullet = FindInactivePooledObject(_playerBulletPool, obj.name);
        }
        
        if (bullet != null)
        {
            // プールから再利用
            bullet.transform.position = pos;
            bullet.transform.rotation = rot;
            bullet.SetActive(true);
            //Debug.Log($"プレイヤー弾を再利用: {obj.name}");
        }
        else
        {
            // 新規作成
            bullet = Instantiate(obj, pos, rot);
            if (_playerBulletPool != null)
            {
                bullet.transform.SetParent(_playerBulletPool.transform);
            }
            //Debug.Log($"プレイヤー弾を新規作成: {obj.name}");
        }
        
        return bullet;
    }

    /// <summary>
    /// 敵の弾を再生する（オブジェクトプーリング対応）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public GameObject PlayEnemyBullet(GameObject obj, Vector3 pos, Quaternion rot){
        GameObject bullet = null;
        
        // _enemyBulletPoolから同じ名前の非アクティブなオブジェクトを探す
        if (_enemyBulletPool != null)
        {
            bullet = FindInactivePooledObject(_enemyBulletPool, obj.name);
        }
        
        if (bullet != null)
        {
            // プールから再利用
            bullet.transform.position = pos;
            bullet.transform.rotation = rot;
            bullet.SetActive(true);
            Debug.Log($"弾を再利用: {obj.name}");
        }
        else
        {
            // 新規作成
            bullet = Instantiate(obj, pos, rot);
            if (_enemyBulletPool != null)
            {
                bullet.transform.SetParent(_enemyBulletPool.transform);
            }
            Debug.Log($"弾を新規作成: {obj.name}");
        }
        
        return bullet;
    }
    
    /// <summary>
    /// プールから指定した名前の非アクティブなオブジェクトを探す
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="objectName"></param>
    /// <returns></returns>
    GameObject FindInactivePooledObject(GameObject pool, string objectName)
    {
        for (int i = 0; i < pool.transform.childCount; i++)
        {
            Transform child = pool.transform.GetChild(i);
            
            // 名前が一致し、かつ非アクティブなオブジェクトを探す
            if (child.name.Contains(objectName) && !child.gameObject.activeInHierarchy)
            {
                return child.gameObject;
            }
        }
        
        return null; // 見つからない場合
    }

    /// <summary>
    /// プールされている全ての弾を非アクティブにする
    /// </summary>
    public void DeactivateAllPooledBullets()
    {
        int playerBulletCount = DeactivatePoolChildren(_playerBulletPool);
        int enemyBulletCount = DeactivatePoolChildren(_enemyBulletPool);
        
        Debug.Log($"プレイヤー弾を{playerBulletCount}個非アクティブ化、敵弾を{enemyBulletCount}個非アクティブ化しました");
    }
    
    /// <summary>
    /// 指定したプールの子オブジェクトでアクティブなものを全て非アクティブにする
    /// </summary>
    /// <param name="pool">対象のプール</param>
    /// <returns>非アクティブ化したオブジェクトの数</returns>
    int DeactivatePoolChildren(GameObject pool)
    {
        if (pool == null)
        {
            return 0;
        }
        
        int deactivatedCount = 0;
        
        for (int i = 0; i < pool.transform.childCount; i++)
        {
            Transform child = pool.transform.GetChild(i);
            
            if (child.gameObject.activeInHierarchy)
            {
                child.gameObject.SetActive(false);
                deactivatedCount++;
            }
        }
        
        return deactivatedCount;
    }

    /// <summary>
    /// エフェクトを見た目正しい位置に表示する
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 GetCorrectedPosition(Vector3 pos){
        if (effectCamera != null && mainCamera != null)
        {
            return GetCorrectedPosition(mainCamera, effectCamera, pos);
        }
        return pos;
    }

    /// <summary>
    /// Orthographic カメラ上での見え位置に合わせて、
    /// Perspective カメラで同じスクリーン位置に出力されるワールド座標を返す
    /// </summary>
    Vector3 GetCorrectedPosition(
        Camera orthoCam,
        Camera perspCam,
        Vector3 worldPos
    ) {
        // 1) Orthographic カメラでワールド→スクリーン座標に変換
        Vector3 orthoScreen = orthoCam.WorldToScreenPoint(worldPos);

        // 2) Depth（Z）だけは Perspective カメラでの距離を拾ってくる
        //    こうすると、本来のオブジェクト位置からのカメラ距離を保持できる
        float depth = perspCam.WorldToScreenPoint(worldPos).z;

        // 3) スクリーン座標の Z に depth をセット
        orthoScreen.z = depth;

        // 4) Perspective カメラでスクリーン→ワールド座標に逆変換
        Vector3 correctedWorld = perspCam.ScreenToWorldPoint(orthoScreen);

        return correctedWorld;
    }
    



}
