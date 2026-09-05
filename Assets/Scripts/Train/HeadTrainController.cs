using System.Collections.Generic;
using UnityEngine;

public class HeadTrainController : TrainController
{
    [SerializeField] List<TrainController> vagons = new();
    [SerializeField] HeadTrainModel oppositeCabin;

    public void SetActive(bool active)
    {
        ((HeadTrainModel)trainModel).SetActive(active);
        oppositeCabin.UpdateLights();
    }
    public List<TrainController> GetVagonsList() => vagons;
    public void ChangeLeftDoorState(bool open)
    {
        if (open) OpenLeftDoors();
        else CloseLeftDoors();
    }

    public void ChangeRightDoorState(bool open)
    {
        if (open) OpenRightDoors();
        else CloseRightDoors();
    }

    public void OpenLeftDoors()
    {
        foreach(var vagon in vagons)
        {
            vagon._OpenLeftDoors();
        }
    }

    public void OpenRightDoors()
    {
        foreach (var vagon in vagons)
        {
            vagon._OpenRightDoors();
        }
    }

    public void CloseLeftDoors()
    {
        foreach (var vagon in vagons)
        {
            vagon._CloseLeftDoors();
        }
    }

    public void CloseRightDoors()
    {
        foreach (var vagon in vagons)
        {
            vagon._CloseRightDoors();
        }
    }

    public void SetBraking(bool braking)
    {
        foreach (var vagon in vagons)
        {
            vagon._SetBraking(braking);
        }
    }

    public void SetRegularLightState(bool newState)
    {
        foreach (var vagon in vagons)
        {
            vagon._SetRegularLightState(newState);
        }
    }

    public void SetCabinLightState(bool newState)
    {
        ((HeadTrainModel)trainModel).SetCabinLightEnabled(newState);
    }
}
