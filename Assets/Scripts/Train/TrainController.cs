using UnityEngine;

[RequireComponent(typeof(TrainModel))]
public class TrainController : MonoBehaviour
{
    protected TrainModel trainModel;

    private void Start()
    {
        trainModel = GetComponent<TrainModel>();
    }

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

    public void _SetBraking(bool braking)
    {
        trainModel.SetBraking(braking);
    }
}
