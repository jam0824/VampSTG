using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    /// <summary>
    /// クリア後のリザルト画面で呼ばれる
    /// </summary>
    /// <param name="nowStageName"></param>
    public void UnlockStage(string nowStageName){
        nowStageName = nowStageName.ToLower();
        switch(nowStageName){
            case "stage1":
                GameManager.Instance.AddStage("stage2");
                break;
            case "stage2":
                GameManager.Instance.AddStage("stage3");
                break;
            case "stage3":
                GameManager.Instance.AddStage("stage4");
                break;
            case "stage4":
                GameManager.Instance.AddStage("stage5");
                break;
            case "stage5":
                GameManager.Instance.AddStage("stage6");
                break;
        }

    }
}
