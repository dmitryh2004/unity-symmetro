using UnityEngine;

public class TrainSittableObject : Interactable
{
    [SerializeField] int seatIndex;
    [SerializeField] TrainSeat seatObject;
    PlayerMovement player = null;

    private void Start()
    {
        player = GameObject.FindFirstObjectByType<PlayerMovement>();
    }

    public override void Interact()
    {
        if (player != null) seatObject.SitDown(seatIndex, player);
    }
}
