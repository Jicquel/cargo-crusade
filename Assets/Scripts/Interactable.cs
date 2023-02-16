using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact();
    [SerializeField]
    protected GameObject interactIcon = null;

    public void DisableInteractIcon()
    {
        if(interactIcon != null)
        {
            interactIcon?.SetActive(false);
        }
    }

    public void EnableInteractIcon()
    {
        if(interactIcon != null)
        {
            interactIcon?.SetActive(true);
        }
    }
}
