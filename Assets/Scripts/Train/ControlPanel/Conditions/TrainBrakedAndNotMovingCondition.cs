using UnityEngine;

public class TrainBrakedAndNotMovingCondition : IBoolCondition
{
    [SerializeField] HeadTrainModel trainModel;
    public override bool Check()
    {
        return trainModel.IsBraking() && (trainModel.GetCurrentSpeed() < 0.01f);
    }
}
