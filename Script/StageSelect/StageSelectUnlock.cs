using UnityEngine;
using UnityEngine.UI;

public class StageSelectUnlock : MonoBehaviour
{
    [Header("ステージ名（アンロック時の名前と合わせること）")]
    [SerializeField] private string stageName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnlockStage(stageName);
    }

    void UnlockStage(string stageName){
        if(GameManager.Instance.IsStageUnlocked(stageName)){
            gameObject.GetComponent<Button>().interactable = true;
        }
        else{
            gameObject.GetComponent<Button>().interactable = false;
        }
    }

    
}
