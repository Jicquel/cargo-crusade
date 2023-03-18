using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralController : Interactable
{

    override
    public Interactable Interact()
    {
        Debug.Log("Interacting!");
        Destroy(this.gameObject, 1);
        return null;
    }


}
