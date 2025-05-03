using UnityEngine;

public class PlayerEffectObj : MonoBehaviour
{
    //プレイヤーに追従するだけのオブジェクト
    //プレイヤーの点滅でエフェクトがerrorになるので作成

    private GameObject core;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(core == null){
            core = GameObject.FindWithTag("Core");
        }
        gameObject.transform.position = core.gameObject.transform.position;
        
    }
}
