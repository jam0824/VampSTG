using UnityEngine;
using System.Collections.Generic;

public class InnerBgManager : MonoBehaviour, IScrollSpeed
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
    [SerializeField] private string floorTag = "Ground";
    [Header("Scroll Speed")]
    [SerializeField] public float scrollSpeed = 1f;
    [Header("BossParts")]
    [SerializeField] private GameObject bossParts;
    [SerializeField] private float bossPartsY = -4.2f;
    [Header("ボスが出る何秒手前でボス部屋を生成するか")]
    [SerializeField] private float bossRoomBeforeTime = 10f;
    // IScrollSpeedインターフェースの実装
    public virtual float ScrollSpeed => scrollSpeed;
    private StageManager stageManager;
    private bool isMakingStop = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float startZ = -10f;
    private int startTimes = 5;

    private float zPosition = 0f;
    private float width = 10f;
    void Start()
    {
        CreateInnerBg(startZ, startTimes);
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isMakingStop) return;
        scrollSpeed = stageManager.scrollSpeed;
        zPosition += scrollSpeed * Time.deltaTime;
        if(zPosition >= width){
            if(stageManager.allElapsedTime >= stageManager.stageAllSecond - bossRoomBeforeTime){
                CreateBossRoom(20f, bossPartsY);
            }else{
                CreateInnerBg(30f, 1);
            }
            zPosition = 0f;
        }
    }

    public void CreateInnerBg(float startZ,int times)
    {
        for(int i = 0; i < times; i++){
            Vector3 position = new Vector3(0f, 0f, startZ + i * width);
            GameObject bgRoot = Instantiate(innerBgRoot, position, Quaternion.identity);
            MakeInnerBg makeInnerBg = bgRoot.GetComponent<MakeInnerBg>();
            makeInnerBg.scrollSpeed = scrollSpeed;
            makeInnerBg.listWallParts = listInnerBgParts;
            makeInnerBg.bigWallParts = bigWallParts;
            makeInnerBg.floorParts = floorParts;
            makeInnerBg.floorTag = floorTag;
            makeInnerBg.CreateVerticalWallParts(bgRoot.transform.position.z);
            makeInnerBg.CreateFloorParts(bgRoot.transform.position.z, floorY);
        }
    }

    private void CreateBossRoom(float startZ, float y){
        Vector3 position = new Vector3(0f, y, startZ);
        GameObject bgRoot = Instantiate(bossParts, position, Quaternion.identity);
        MakeInnerBg makeInnerBg = bgRoot.GetComponent<MakeInnerBg>();
        makeInnerBg.scrollSpeed = scrollSpeed;
        makeInnerBg.isDestroy = false;
        isMakingStop = true;
    }
}
