using UnityEngine;

[RequireComponent(typeof(TrainModel))]
public class TrainController : MonoBehaviour
{
    protected TrainModel trainModel;

    private void Awake()
    {
        trainModel = GetComponent<TrainModel>();
    }

    public TrainModel GetTrainModel() => trainModel;

    public void _OpenLeftDoors()
    {
        if (!trainModel.LeftDoorsOpened())
            trainModel.SetLeftDoorsOpened(true);
    }

    public void _OpenRightDoors()
    {
        if (!trainModel.RightDoorsOpened())
            trainModel.SetRightDoorsOpened(true);
    }

    public void _CloseLeftDoors()
    {
        if (trainModel.LeftDoorsOpened())
            trainModel.SetLeftDoorsOpened(false);
    }

    public void _CloseRightDoors()
    {
        if (trainModel.RightDoorsOpened())
            trainModel.SetRightDoorsOpened(false);
    }

    public void _SetRegularLightState(bool newState)
    {
        trainModel.SetRegularLampsState(newState);
    }

    public void _SetBraking(bool braking)
    {
        trainModel.SetBraking(braking);
    }
}
