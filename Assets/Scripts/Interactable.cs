using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]

public abstract class Interactable : MonoBehaviour
{
    // Behavior when interacting with the GameObject.
    // Return updated Interactable (null if Gameobject 
    // is or will eventually be deleted.
    
    public abstract Interactable Interact();
    [SerializeField]
    protected GameObject interactIcon = null;
    private SpriteRenderer _spriteRenderer;
    private Color _color;


    private void Awake()
    {
       _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DisableInteractIcon()
    {
        _spriteRenderer.color = _color;
        if(interactIcon != null)
        {
            interactIcon?.SetActive(false);
        }
    }

    public void EnableInteractIcon()
    {
        _color = _spriteRenderer.color;
        _spriteRenderer.color = Color.red;
        if(interactIcon != null)
        {
            interactIcon?.SetActive(true);
        }
    }
}
