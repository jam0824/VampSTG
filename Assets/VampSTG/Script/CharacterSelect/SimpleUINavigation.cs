using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // ← ここを必ず追加！

public class SimpleUINavigation : MonoBehaviour
{
    public float repeatDelay = 0.3f;
    float timerX, timerY;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (x != 0 && Time.time > timerX)
        {
            Move(new Vector2(x, 0));
            timerX = Time.time + repeatDelay;
        }
        if (y != 0 && Time.time > timerY)
        {
            Move(new Vector2(0, y));
            timerY = Time.time + repeatDelay;
        }
    }

    void Move(Vector2 dir)
    {
        var currentGO = EventSystem.current.currentSelectedGameObject;
        if (currentGO == null) return;

        // Button や Toggle は Selectable 派生なので、Selectable を取得
        var selectable = currentGO.GetComponent<Selectable>();
        if (selectable == null) return;

        // Navigation は構造体なので、Selectable.navigation からアクセス
        var nav = selectable.navigation;
        GameObject next = null;

        if (dir.x > 0)
            next = nav.selectOnRight?.gameObject;
        else if (dir.x < 0)
            next = nav.selectOnLeft?.gameObject;
        else if (dir.y > 0)
            next = nav.selectOnUp?.gameObject;
        else if (dir.y < 0)
            next = nav.selectOnDown?.gameObject;

        if (next != null)
            EventSystem.current.SetSelectedGameObject(next);
    }
}
