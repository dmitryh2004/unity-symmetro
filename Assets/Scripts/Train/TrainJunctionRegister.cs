using UnityEngine;

public class TrainJunctionRegister : MonoBehaviour
{
    [SerializeField] TrainModel model;

    public TrainModel GetModel() => model;
}
