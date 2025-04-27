using UnityEngine;

public class PlayerEffectObj : MonoBehaviour
{
    //プレイヤーに追従するだけのオブジェクト
    //プレイヤーの点滅でエフェクトがerrorになるので作成

    private GameObject player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null){
            player = GameObject.FindWithTag("Player");
        }
        gameObject.transform.position = player.gameObject.transform.position;
        
    }
}
