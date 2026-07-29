using System.Collections.Generic;
using UnityEngine;

public class AnotherCabinIsNotActiveCondition : IBoolCondition
{
    [SerializeField] HeadTrainController headTrainController;
    HeadTrainModel anotherCabin;

    private void Start()
    {
        List<TrainController> vagons = headTrainController.GetVagonsList();
        foreach (var vagon in vagons)
        {
            if (vagon is HeadTrainController headTrain)
            {
                if (headTrain != headTrainController)
                {
                    anotherCabin = ((HeadTrainModel)headTrain.GetTrainModel());
                    return;
                }
            }
        }

        Debug.LogError($"{gameObject.name}: another cabin is not found");
    }

    public override bool Check()
    {
        if (anotherCabin != null) return !anotherCabin.IsActive();
        return false; // не можем сказать, так как вторая кабина не найдена
    }
}
