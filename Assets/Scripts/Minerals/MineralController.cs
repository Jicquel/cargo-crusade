using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralController : Interactable
{
    Color _initialColor;
    SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
    }

    override
    public void Interact()
    {
        if(_spriteRenderer.color == Color.red)
        {
            _spriteRenderer.color = _initialColor;
        }
        else { 
            _spriteRenderer.color = Color.red;
        }
    }


}
