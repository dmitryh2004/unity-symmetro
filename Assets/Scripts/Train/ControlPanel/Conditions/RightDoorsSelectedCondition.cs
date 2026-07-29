using UnityEngine;

public class RightDoorsSelectedCondition : IBoolCondition
{
    [SerializeField] ControlPanelElementController doorSelector;

    public override bool Check()
    {
        return doorSelector.GetCurrentState() == true;
    }
}
