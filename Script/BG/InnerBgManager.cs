using UnityEngine;
using System.Collections.Generic;

public class InnerBgManager : MonoBehaviour
{
    [Header("InnerBgRoot")]
    [SerializeField] private GameObject innerBgRoot;
    [Header("InnerBgParts")]
    [SerializeField] private List<GameObject> listInnerBgParts;
    [Header("Big Wall Parts")]
    [SerializeField] private List<GameObject> bigWallParts;
    [Header("FloorParts")]
    [SerializeField] private GameObject floorParts;
    [SerializeField] private float floorY = -4.2f;
    [Header("Scroll Speed")]
    [SerializeField] public float scrollSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float startZ = -10f;
    private int startTimes = 5;

    private float zPosition = 0f;
    void Start()
    {
        CreateInnerBg(startZ, startTimes);
    }

    // Update is called once per frame
    void Update()
    {
        zPosition += scrollSpeed * Time.deltaTime;
        if(zPosition >= 10f){
            CreateInnerBg(30f, 1);
            zPosition = 0f;
        }
    }

    public void CreateInnerBg(float startZ,int times)
    {
        for(int i = 0; i < times; i++){
            GameObject bgRoot = Instantiate(innerBgRoot);
            MakeInnerBg makeInnerBg = bgRoot.GetComponent<MakeInnerBg>();
            makeInnerBg.scrollSpeed = scrollSpeed;
            makeInnerBg.listWallParts = listInnerBgParts;
            makeInnerBg.bigWallParts = bigWallParts;
            makeInnerBg.floorParts = floorParts;
            makeInnerBg.CreateVerticalWallParts(startZ + i * 10f);
            makeInnerBg.CreateFloorParts(startZ + i * 10f, floorY);
        }
    }
}
