using UnityEngine;

[RequireComponent(typeof(TrainModel))]
public class TrainController : MonoBehaviour
{
    TrainModel trainModel;

    private void Start()
    {
        trainModel = GetComponent<TrainModel>();
    }
}
