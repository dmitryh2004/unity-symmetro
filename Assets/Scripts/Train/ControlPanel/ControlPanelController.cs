using UnityEngine;

public class ControlPanelController : MonoBehaviour
{
    [SerializeField] HeadTrainModel linkedModel;
    [SerializeField] ControlPanelElementController doorsLeft, doorsRight;
    [SerializeField] ControlPanelSvetodiodController doorsLeft_s, doorsRight_s;
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
}
