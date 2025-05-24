using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUnlock : MonoBehaviour
{
    [Header("キャラクター名（アンロック時の名前と合わせること）")]
    [SerializeField] private string characterName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnlockCharacter(characterName);
    }

    void UnlockCharacter(string characterName){
        if(GameManager.Instance.IsCharacterUnlocked(characterName)){
            gameObject.GetComponent<Button>().interactable = true;
        }
        else{
            gameObject.GetComponent<Button>().interactable = false;
        }
    }
}
