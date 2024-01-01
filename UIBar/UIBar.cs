using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 이 스크립트는 ui에 넣는 스크립트
/// 이 ui 위에서 마우스가 다운될 시 다시 마우스를 올릴 때까지 따라다님 
/// 아무 호출도 필요하지 않고 스크립트만 넣으면 됨
/// </summary>
public class UIBar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Vector2 m_Difference;
    Coroutine m_Coroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        m_Difference = Input.mousePosition - transform.position;
        if(m_Coroutine != null )
            StopCoroutine( m_Coroutine );
        m_Coroutine = StartCoroutine(FllowMouse());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopCoroutine(m_Coroutine);
    }

    private IEnumerator FllowMouse()
    {
        while (true)
        {
            transform.position = (Vector2)Input.mousePosition - m_Difference;

            yield return null;
        }
    }
}
