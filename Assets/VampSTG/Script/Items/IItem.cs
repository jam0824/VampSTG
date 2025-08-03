using UnityEngine;
using UnityEngine.Rendering;

public interface IItem
{
    string itemType{get;}
    int batteryLevel{get; set;}
    void getItem();

    bool SetActive(bool isActive);

    bool StopAllCoroutine();

    
}
