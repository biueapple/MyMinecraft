using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �� ��ũ��Ʈ�� ui�� �ִ� ��ũ��Ʈ
/// �� ui ������ ���콺�� �ٿ�� �� �ٽ� ���콺�� �ø� ������ ����ٴ� 
/// �ƹ� ȣ�⵵ �ʿ����� �ʰ� ��ũ��Ʈ�� ������ ��
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
