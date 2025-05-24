using UnityEngine;

public class PrisonerConfig : MonoBehaviour
{
    [Header("CharacterData")]
    public CharacterData characterData;
    [Header("取得フラグ")]
    public bool isGet = false;

    public string GetCharacterId()
    {
        return characterData.characterId;
    }
    
}
