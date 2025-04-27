using UnityEngine;

public interface IItem
{
    string itemType{get;}
    int batteryLevel{get; set;}
    void getItem();

    
}
