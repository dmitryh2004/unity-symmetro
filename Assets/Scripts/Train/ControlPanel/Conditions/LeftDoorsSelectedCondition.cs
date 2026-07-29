using UnityEngine;

public class LeftDoorsSelectedCondition : IBoolCondition
{
    [SerializeField] ControlPanelElementController doorSelector;

    public override bool Check()
    {
        return doorSelector.GetCurrentState() == false;
    }
}
