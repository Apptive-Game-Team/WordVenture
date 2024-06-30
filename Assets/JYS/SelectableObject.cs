using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private Vector3 Scale;
    private bool selectable = false;
    CombineZone combineZone;

    private void Start()
    {
        Scale = transform.localScale;
        combineZone = GameObject.Find("CombineZone").GetComponent<CombineZone>();
    }

    public bool GetSelectable()
    {
        return selectable;
    }

    public void SetSelectable(bool selectable)
    {
        this.selectable = selectable;
    }

    private void OnMouseEnter()
    {
        if (selectable)
        {
            ChangeSize(true);
        }   
    }

    private void OnMouseDown()
    {
        if (selectable)
        {
            combineZone.SetTarget(this);
        }
    }

    private void OnMouseExit()
    {
        ChangeSize(false);
    }


    private void ChangeSize(bool bigSide)
    {
        if (bigSide)
            transform.localScale = Scale * 1.2f;
        else
            transform.localScale = Scale;
    }
}
