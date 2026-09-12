using UnityEngine;

public class TrainCabinSeat : TrainSeat
{
    [SerializeField] TrainEngine trainEngine;
    [SerializeField] HeadTrainController htc;
    protected override void OnSatDown(SeatableEntity entity)
    {
        base.OnSatDown(entity);
        if (entity is PlayerMovement player) {
            player.SetTrainCabinLinks(trainEngine, htc);
        }
    }

    protected override void OnStoodUp(SeatableEntity entity)
    {
        base.OnStoodUp(entity);
        if (entity is PlayerMovement player) {
            player.ClearTrainCabinLinks();
        }
    }
}
