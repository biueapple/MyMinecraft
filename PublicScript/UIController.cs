using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    //GraphicRaycaster
    GraphicRaycaster m_gr;
    PointerEventData m_ped;
    List<RaycastResult> results;

    private Canvas canvas;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        m_ped = new PointerEventData(null);
        m_gr = canvas.GetComponent<GraphicRaycaster>();
        results = new List<RaycastResult>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator FadeOut(Graphic graphic, float wait, float fade, bool destroy)
    {
        graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1);
        yield return new WaitForSeconds(wait);
        float t = 1;
        while (true)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, t);
            yield return null;
            t -= Time.deltaTime / fade;
            if (t < 0)
                break;
        }
        if (destroy)
            Destroy(graphic.gameObject);
    }

    public IEnumerator MoveUpCoroutine(Graphic graphic, Vector3 start, float speed, Camera cam)
    {
        graphic.rectTransform.anchoredPosition = cam.WorldToScreenPoint(start);
        while (graphic != null)
        {
            graphic.rectTransform.anchoredPosition += Vector2.up * Time.deltaTime * speed;
            yield return null;
        }
    }

    public IEnumerator FollowUICoroutine(Graphic graphic, GameObject obj, Vector2 plus, Camera cam)
    {
        while (graphic != null || obj != null)
        {
            graphic.rectTransform.anchoredPosition = cam.WorldToScreenPoint(obj.transform.position);
            graphic.rectTransform.anchoredPosition += plus;

            yield return null;
        }
    }

    public IEnumerator MouseFallow(GameObject graphic)
    {
        while(true)
        {
            graphic.transform.position = Input.mousePosition;

            yield return null;
        }
    }

    public void TouchUI(GameObject obj)
    {
        obj.transform.SetAsLastSibling();
    }

    public Transform GetGraphicRay()
    {
        results.Clear();

        m_ped.position = Input.mousePosition;

        m_gr.Raycast(m_ped, results);

        if (results.Count > 0)
        {
            return results[0].gameObject.transform;
        }
        return null;
    }
    public T GetGraphicRay<T>()     //가장 첫번째 ui가 T인가
    {
        results.Clear();

        m_ped.position = Input.mousePosition;

        m_gr.Raycast(m_ped, results);

        if (results.Count > 0)
        {
            return results[0].gameObject.GetComponent<T>();
        }
        return default(T);
    }
    public T GetGraphicRay<T>(bool b)       //T를 찾아서 리턴
    {
        results.Clear();

        m_ped.position = Input.mousePosition;

        m_gr.Raycast(m_ped, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.transform.GetComponent<T>() != null)
                {
                    TouchUI(results[i].gameObject);
                    return results[i].gameObject.GetComponent<T>();
                }
            }
        }
        return default(T);
    }

    public void ListSwap<T>(List<T> list, int index1, int index2)
    {
        if (index2 == -1 || index1 == index2)
            return;

        if (index1 < 0 || index2 >= list.Count)
        {
            return;
        }

        T item = list[index1];
        list[index1] = list[index2];
        list[index2] = item;

    }
    public int FindIndex<T>(List<T> list, T obj)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(obj))
            {
                return i;
            }
        }
        return -1;
    }

    public Canvas GetCanvas()
    {
        return canvas;
    }
}
