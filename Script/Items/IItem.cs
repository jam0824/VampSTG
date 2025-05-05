using UnityEngine;
using UnityEngine.Rendering;

public interface IItem
{
    string itemType{get;}
    int batteryLevel{get; set;}
    void getItem(float powerMagnification);

    bool SetActive(bool isActive);

    bool StopAllCoroutine();

    
}
