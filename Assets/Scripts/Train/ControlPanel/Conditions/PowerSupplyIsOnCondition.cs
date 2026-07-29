using UnityEngine;

public class PowerSupplyIsOnCondition : IBoolCondition
{
    [SerializeField] HeadTrainModel headTrainModel;
    public override bool Check()
    {
        return headTrainModel.IsActive();
    }
}
