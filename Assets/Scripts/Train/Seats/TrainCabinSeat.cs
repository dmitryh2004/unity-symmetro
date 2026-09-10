using UnityEngine;

public class TrainCabinSeat : TrainSeat
{
    [SerializeField] TrainEngine trainEngine;
    protected override void OnSatDown(SeatableEntity entity)
    {
        base.OnSatDown(entity);
        if (entity is PlayerMovement player) {
            player.SetTrainEngine(trainEngine);
        }
    }

    protected override void OnStoodUp(SeatableEntity entity)
    {
        base.OnStoodUp(entity);
        if (entity is PlayerMovement player) {
            player.ClearTrainEngine();
        }
    }
}
