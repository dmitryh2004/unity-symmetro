using UnityEngine;

public class TrainBrakedAndNotMovingCondition : IBoolCondition
{
    [SerializeField] HeadTrainModel trainModel;
    public override bool Check()
    {
        return trainModel.IsBraked() && (trainModel.GetCurrentSpeed() < 0.01f);
    }
}
