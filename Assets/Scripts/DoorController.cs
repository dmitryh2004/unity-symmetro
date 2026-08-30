using UnityEngine;

public class DoorController : Interactable
{
    bool opened = false;
    [SerializeField] Animator anim;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        opened = !opened;
        if (anim != null) anim.SetBool("opened", opened);
    }
}
