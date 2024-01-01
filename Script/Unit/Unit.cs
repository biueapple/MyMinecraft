using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    protected Stat stat;
    public Stat STAT { get { return stat; } }
    protected MoveSystem moveSystem;

    public Transform model;
    protected Color originalColor = Color.gray;
    protected Color hitColor = Color.red;
    protected Coroutine colorCoroutine = null;

    protected int level;

    protected void Start()
    {
        stat = GetComponent<Stat>();
        moveSystem = GetComponent<MoveSystem>();
        originalColor = model.GetComponent<Renderer>().material.GetColor("_Color");
    }

    protected void Update()
    {
        if (stat.Attacktimer <= stat.AttackSpeed)
            stat.Attacktimer += Time.deltaTime;
    }

    public virtual void Hit(Stat perpetrator, float figure, ATTACKTYPE attack, DAMAGETYPE damage)
    {
        stat.Be_Attacked(perpetrator, figure, attack, damage);
        Vector3 dir = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(perpetrator.transform.position.x, 0, perpetrator.transform.position.z)).normalized;
        moveSystem.jumpMomemtum = 4;
        moveSystem.ApplyExternalForce(dir * 5);
        model.GetComponent<Renderer>().material.SetColor("_Color", hitColor);
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(HitColor());
    }

    protected IEnumerator HitColor()
    {
        yield return new WaitForSeconds(0.5f);
        model.GetComponent<Renderer>().material.SetColor("_Color", originalColor);
        colorCoroutine = null;
    }
}
