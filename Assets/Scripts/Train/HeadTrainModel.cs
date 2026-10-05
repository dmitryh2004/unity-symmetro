using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class HeadTrainModel : TrainModel
{
    [Header("Head part management")]
    [SerializeField] bool active;
    [SerializeField] bool lightsEnabled;
    [SerializeField] bool forceInitializeVagons = false;
    [SerializeField] List<LightRenderController> headLights = new ();
    [SerializeField] List<LightEmissivePartController> headLightControllers = new ();
    [SerializeField] List<LightEmissivePartController> upperHeadLightControllers = new ();

    [SerializeField] TrainEngine engine;
    [SerializeField] TrainLampController cabinLight;
    [SerializeField] ControlPanelController controlPanelController;
    bool cabinLightEnabled = false;

    [SerializeField] public List<TrainModel> chainedVagons = new();

    private List<PathPoint> pathHistory = new List<PathPoint>();
    private float totalDrivenDistance = 0f;

    [Header("Настройки оптимизации истории")]
    [Tooltip("Записывать точку, если поезд проехал это расстояние (в метрах)")]
    [SerializeField] private float recordingDistanceStep = 0.25f;

    [Tooltip("Записывать точку, если поезд повернулся на этот угол (в градусах)")]
    [SerializeField] private float recordingAngleStep = 2.0f;

    [Header("Head panel")]
    [SerializeField] SpeedController speedController;

    protected override void Init()
    {
        base.Init();
        SetActive(active);
        if (forceInitializeVagons)
        {
            InitializeVagons();
        }
    }
    private void InitializeVagons()
    {
        if (rail == null || rail.Spline == null) return;
        currentSpline = rail.Spline;

        InitializeCurrentSpline();

        ResetHistory();
        totalDrivenDistance = 0f;
        var native = new NativeSpline(currentSpline);

        float totalTrainLength = 0f;
        foreach (TrainModel vagon in chainedVagons)
        {
            if (vagon != this) totalTrainLength += vagonSpacing;
        }

        float initStep = 0.2f;
        float headDirectionSign = GetSplineDirectionSign();

        // Считаем, в какую сторону по сплайну от этой кабины выстроены остальные вагоны.
        // Если кабина инвертирована, то хвост поезда находится в ПЛЮСОВОЙ стороне сплайна относительно неё.
        float historyDirectionModifier = IsInvertRotation() ? 1f : -1f;

        // Генерируем точки от самого дальнего вагона к текущей кабине (0)
        // Если historyDirectionModifier > 0, то цикл идет от totalTrainLength до 0 с шагом -initStep
        float startD = IsInvertRotation() ? totalTrainLength : -totalTrainLength;
        float endD = 0f;
        float step = IsInvertRotation() ? -initStep : initStep;

        // Для удобства используем обычный for, проходя по абстрактным шагам
        int stepsCount = Mathf.CeilToInt(totalTrainLength / initStep);

        for (int i = stepsCount; i >= 0; i--)
        {
            float d = i * initStep * historyDirectionModifier;
            float splineDist = currentSplineDistance + (d * headDirectionSign);

            if (native.Closed)
            {
                float length = native.GetLength();
                splineDist = (splineDist % length + length) % length;
            }
            else
            {
                splineDist = Mathf.Clamp(splineDist, 0f, native.GetLength());
            }

            float t = native.ConvertIndexUnit(splineDist, PathIndexUnit.Distance, PathIndexUnit.Normalized);

            Vector3 localPos = native.EvaluatePosition(t);
            Vector3 worldPos = rail.transform.TransformPoint(localPos);

            Vector3 splineTangent = Vector3.Normalize(native.EvaluateTangent(t));
            Vector3 splineUp = native.EvaluateUpVector(t);
            Vector3 worldForward = rail.transform.TransformDirection(splineTangent * headDirectionSign);
            Vector3 worldUp = rail.transform.TransformDirection(splineUp);

            var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
            Quaternion baseWorldRot = Quaternion.LookRotation(worldForward, worldUp) * axisRemapRotation;

            pathHistory.Add(new PathPoint
            {
                position = worldPos,
                rotation = baseWorldRot,
                // Дистанция в истории всегда должна расти от хвоста к голове.
                // Для инвертированной кабины d положительный, поэтому инвертируем знак, 
                // чтобы у хвоста всегда было меньшее значение, чем у головы.
                distanceAtPoint = IsInvertRotation() ? -d : d
            });
        }

        // Принудительная расстановка вагонов на старте
        float accumulatedOffset = 0f;
        foreach (TrainModel vagon in chainedVagons)
        {
            if (vagon == this) continue;

            accumulatedOffset += vagonSpacing;
            // Хвост всегда ищет свои точки в отрицательной (прошлой) области относительно головы (0)
            float targetVagonAbsoluteDistance = -accumulatedOffset;

            PathPoint vagonPoint = GetPointFromHistory(targetVagonAbsoluteDistance);
            Quaternion finalVagonRot = CalculateVagonRotation(vagon, vagonPoint.rotation);

            Debug.Log($"{gameObject.name} - Setted {vagon.gameObject.name} position to {vagonPoint.position}");

            vagon.transform.position = vagonPoint.position;
            vagon.transform.rotation = finalVagonRot;

            var vagonRb = vagon.GetComponent<Rigidbody>();
            if (vagonRb != null)
            {
                vagonRb.position = vagonPoint.position;
                vagonRb.rotation = finalVagonRot;
            }
        }
    }


    private void FixedUpdate()
    {
        // ЕСЛИ МЫ НЕ ВЕДУЩИЙ ВАГОН, ОТКЛЮЧАЕМ ЛОГИКУ ДВИЖЕНИЯ И ИСТОРИИ!
        // Это предотвратит конфликт двух кабин.
        if (!active) return;

        // 1. Физика активной головы
        float newSpeedMagnitude = Mathf.Clamp(Mathf.Abs(currentSpeedMagnitude) - frictionDeceleration * Time.fixedDeltaTime, 0f, 25f);
        float speedSign = Mathf.Sign(currentSpeedMagnitude);
        currentSpeedMagnitude = newSpeedMagnitude * speedSign;

        if (IsBraked() && Mathf.Abs(currentSpeedMagnitude) > 0f)
        {
            float brakeDecel = brakingDeceleration * GetBrakeStrength() * Time.fixedDeltaTime;
            float speedAfterBraking = Mathf.Clamp(Mathf.Abs(currentSpeedMagnitude) - brakeDecel, 0f, 25f);
            currentSpeedMagnitude = speedAfterBraking * speedSign;
        }

        currentSpeed = transform.forward * currentSpeedMagnitude;

        float headDirectionSign = GetSplineDirectionSign();

        // ВАЖНО: Если кабина инвертирована (IsInvertRotation = true), то движение "вперед" 
        // для физики вагона означает движение НАЗАД по направлению сплайна.
        float driveDirectionModifier = IsInvertRotation() ? -1f : 1f;

        float deltaDistance = currentSpeedMagnitude * Time.fixedDeltaTime * headDirectionSign * driveDirectionModifier;
        currentSplineDistance += deltaDistance;

        // Расчет позиции на сплайне
        var native = new NativeSpline(currentSpline);
        float t = native.ConvertIndexUnit(currentSplineDistance, PathIndexUnit.Distance, PathIndexUnit.Normalized);

        Vector3 localPosition = native.EvaluatePosition(t);
        Vector3 worldPosition = rail.transform.TransformPoint(localPosition);

        Vector3 splineTangent = Vector3.Normalize(native.EvaluateTangent(t));
        Vector3 splineUp = native.EvaluateUpVector(t);
        Vector3 worldForward = rail.transform.TransformDirection(splineTangent * headDirectionSign);
        Vector3 worldUp = rail.transform.TransformDirection(splineUp);

        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
        Quaternion baseWorldRotation = Quaternion.LookRotation(worldForward, worldUp) * axisRemapRotation;

        Quaternion finalHeadRotation = IsInvertRotation() ? baseWorldRotation * Quaternion.Euler(0, 180, 0) : baseWorldRotation;

        rb.MovePosition(worldPosition);
        rb.MoveRotation(finalHeadRotation);

        // 2. ДИНАМИЧЕСКАЯ ЗАПИСЬ ИСТОРИИ
        totalDrivenDistance += Mathf.Abs(deltaDistance);

        RecordCustomPosition(worldPosition, baseWorldRotation, totalDrivenDistance, forceRecord: false);

        // 3. ПОЗИЦИОНИРОВАНИЕ ВАГОНОВ ИЗ ИСТОРИИ
        float accumulatedOffset = 0f;
        foreach (TrainModel vagon in chainedVagons)
        {
            if (vagon == this) continue;

            accumulatedOffset += vagonSpacing;
            float targetVagonAbsoluteDistance = totalDrivenDistance - accumulatedOffset;

            PathPoint vagonPoint = GetPointFromHistory(targetVagonAbsoluteDistance);

            Quaternion finalVagonRot = CalculateVagonRotation(vagon, vagonPoint.rotation);
            vagon.MoveVagonExplicitly(vagonPoint.position, finalVagonRot);
        }

        CleanUpHistory(accumulatedOffset + 10f);
    }

    // Вспомогательный метод расчета вращения (остается из прошлого ответа)
    private Quaternion CalculateVagonRotation(TrainModel vagon, Quaternion baseSplineRotation)
    {
        float headDirectionSign = GetSplineDirectionSign();
        float vagonDirectionSign = vagon.GetSplineDirectionSign();

        if (headDirectionSign != vagonDirectionSign)
        {
            baseSplineRotation *= Quaternion.Euler(0, 180, 0);
        }

        if (vagon.IsInvertRotation())
        {
            baseSplineRotation *= Quaternion.Euler(0, 180, 0);
        }

        return baseSplineRotation;
    }

    private void RecordCustomPosition(Vector3 position, Quaternion baseRotation, float absoluteDistance, bool forceRecord)
    {
        if (forceRecord || pathHistory.Count == 0)
        {
            pathHistory.Add(new PathPoint { position = position, rotation = baseRotation, distanceAtPoint = absoluteDistance });
            return;
        }

        PathPoint lastPoint = pathHistory[pathHistory.Count - 1];
        float distanceSinceLastRecord = Mathf.Abs(absoluteDistance - lastPoint.distanceAtPoint);
        float angleSinceLastRecord = Quaternion.Angle(baseRotation, lastPoint.rotation);

        if (distanceSinceLastRecord >= recordingDistanceStep || angleSinceLastRecord >= recordingAngleStep)
        {
            pathHistory.Add(new PathPoint
            {
                position = position,
                rotation = baseRotation,
                distanceAtPoint = absoluteDistance
            });
        }
    }



    // Метод GetPointFromHistory остается прежним (бинарный поиск с Lerp/Slerp)
    private PathPoint GetPointFromHistory(float targetDistance)
    {
        if (pathHistory.Count == 0) return new PathPoint { position = rb.position, rotation = rb.rotation };
        if (targetDistance <= pathHistory[0].distanceAtPoint) return pathHistory[0];
        if (targetDistance >= pathHistory[pathHistory.Count - 1].distanceAtPoint) return pathHistory[pathHistory.Count - 1];

        int low = 0;
        int high = pathHistory.Count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (pathHistory[mid].distanceAtPoint < targetDistance) low = mid + 1;
            else high = mid - 1;
        }

        PathPoint p0 = pathHistory[Mathf.Max(0, low - 1)];
        PathPoint p1 = pathHistory[Mathf.Min(pathHistory.Count - 1, low)];

        float diff = p1.distanceAtPoint - p0.distanceAtPoint;
        float t = (diff > 0f) ? (targetDistance - p0.distanceAtPoint) / diff : 0f;

        return new PathPoint
        {
            position = Vector3.Lerp(p0.position, p1.position, t),
            rotation = Quaternion.Slerp(p0.rotation, p1.rotation, t),
            distanceAtPoint = targetDistance
        };
    }

    private void CleanUpHistory(float maxRequiredOffset)
    {
        float minRequiredDistance = totalDrivenDistance - maxRequiredOffset;
        while (pathHistory.Count > 2 && pathHistory[0].distanceAtPoint < minRequiredDistance)
        {
            pathHistory.RemoveAt(0);
        }
    }

    private void ResetHistory()
    {
        pathHistory.RemoveAll((x) => true);
    }

    public bool IsActive() => active;
    public void SetActive(bool active)
    {
        this.active = active;
        engine.SetActive(active);
        UpdateLights();
        controlPanelController.UpdateState(active);
        if (this.active)
            InitializeVagons();
    }

    public void SetLightsEnabled(bool lightsEnabled)
    {
        this.lightsEnabled = lightsEnabled;
        UpdateLights();
    }

    public void SetCabinLightEnabled(bool enabled)
    {
        cabinLightEnabled = enabled;
    }

    public void UpdateLights()
    {
        bool lightCondition = active && lightsEnabled;
        bool upperLightCondition = IsPoweredUp() && !active;
        foreach (LightRenderController light in headLights)
        {
            light.SetShouldBeEnabled(lightCondition);
        }
        foreach (LightEmissivePartController hlc in headLightControllers)
        {
            if (lightCondition) hlc.Activate(); else hlc.Deactivate();
        }
        foreach (LightEmissivePartController hlc in upperHeadLightControllers)
        {
            if (upperLightCondition) hlc.Activate(); else hlc.Deactivate();
        }
    }

    override protected void UpdateState()
    {
        base.UpdateState();
        speedController.SetSpeedText(GetCurrentSpeed());

        bool cabinLightEnabled = IsPoweredUp() && this.cabinLightEnabled;
        if (cabinLightEnabled != cabinLight.IsActive()) cabinLight.SetState(cabinLightEnabled);
    }
}
