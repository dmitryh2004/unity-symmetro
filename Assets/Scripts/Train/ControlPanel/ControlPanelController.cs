using UnityEngine;

public class ControlPanelController : MonoBehaviour
{
    [SerializeField] HeadTrainModel linkedModel;
    [SerializeField] ControlPanelElementController doorsLeft, doorsRight;
    [SerializeField] ControlPanelSvetodiodController doorsLeft_s, doorsRight_s, speedOch_s, speed0_s, speed40_s, speed60_s, speed70_s, speed80_s;
    [SerializeField] SpeedController speedController;

    public void UpdateState(bool newState)
    {
        if (newState)
        {
            bool leftDoorsOpened = linkedModel.LeftDoorsOpened();
            bool rightDoorsOpened = linkedModel.RightDoorsOpened();

            doorsLeft.SetState(leftDoorsOpened);
            doorsRight.SetState(rightDoorsOpened);

            doorsLeft_s.ChangeState(leftDoorsOpened);
            doorsRight_s.ChangeState(rightDoorsOpened);
        }
        speedController.gameObject.SetActive(newState);
    }

    public void UpdateSpeedSvetodiods()
    {
        
    }
}
