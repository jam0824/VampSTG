using UnityEngine;
using UnityEngine.Rendering;  // BlendMode を使うために追加

public class PrisonerCore : MonoBehaviour
{
    // タグ名を定義
    private const string PRISONER_TAG = "Prisoner";

    [SerializeField] public float hp = 10;
    private float maxHp = 10;
    [SerializeField] float offsetExplosionY = 0f;

    [Header("PlayerCoreに寄ってくる時のパラメーター")]
    public float moveSpeed = 8f;
    public float stopDistance = 0.1f;

    Transform playerTransform;
    bool isDead = false;


    void Start()
    {
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        maxHp = hp;
    }

    void Update()
    {
        if (playerTransform == null) return;

        if ((!isDead) && (hp <= 0)) enemyDie();

        if (isDead)
        {
            if (playerTransform == null)
                playerTransform = GameManager.Instance.playerCore.transform;
            // ─── 移動 ───
            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.x = 0f;
            float dist = toPlayer.magnitude;
            if (dist > stopDistance)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerBullet"))
        {
            if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
            hp = hit(bullet, hp);
            // 近似的に当たり位置を計算
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            if (bullet.triggerEffect != null)
            {
                Instantiate(bullet.triggerEffect, hitPoint, other.gameObject.transform.rotation);
            }
            if (bullet.isDestroy) Destroy(other.gameObject);
        }
        if (hp <= 0) enemyDie();
    }

    float hit(ConfigPlayerBullet bullet, float enemyHp)
    {
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            MakeMaterialTransparent(rend.material);
        }

        transform.parent = null;    // 親を外す
        SetTagToPrisonerRecursive();
        Vector3 pos = transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
        AddScore(maxHp);
    }

    // 標準シェーダーのマテリアルを Fade モードに切り替えて完全透明にするヘルパー
    void MakeMaterialTransparent(Material mat)
    {
        // 1) 描画モードを Fade に
        mat.SetFloat("_Mode", 2);  // 0=Opaque, 1=Cutout, 2=Fade, 3=Transparent
        mat.SetOverrideTag("RenderType", "Transparent");
        // 2) ブレンド設定
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)RenderQueue.Transparent;
        // 3) カラーアルファを 0 に
        Color c = mat.color;
        c.a = 0f;
        mat.color = c;
    }

    void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }

    // 自身とすべての子オブジェクトのタグを "Prisoner" に変更するメソッド
    public void SetTagToPrisonerRecursive()
    {
        // includeInactive=true で非アクティブも含める
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.tag = PRISONER_TAG;
        }
    }

}
