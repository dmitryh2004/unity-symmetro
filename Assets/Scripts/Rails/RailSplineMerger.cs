using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class RailSplineMerger : MonoBehaviour
{
    [Tooltip("Список сплайнов, которые нужно объединить по порядку.")]
    public List<SplineContainer> splinesToMerge = new List<SplineContainer>();

    [Tooltip("Погрешность, в пределах которой точки начала и конца считаются совпадающими.")]
    public float mergeTolerance = 0.01f;

    [Tooltip("Индекс сплайна внутри контейнеров (обычно 0, если в контейнере по одному сплайну).")]
    public int splineIndex = 0;

    public void GenerateMergedSpline()
    {
        if (splinesToMerge == null || splinesToMerge.Count == 0)
        {
            Debug.LogWarning("Список сплайнов для объединения пуст!");
            return;
        }

        // Получаем или добавляем SplineContainer на текущий объект
        if (!TryGetComponent<SplineContainer>(out var targetContainer))
        {
            targetContainer = gameObject.AddComponent<SplineContainer>();
        }

        // Очищаем существующие сплайны в целевом контейнере
        Spline targetSpline = targetContainer.Spline;
        targetSpline.Clear();

        bool isFirstKnot = true;
        Vector3 lastKnotPosition = Vector3.zero;

        // Список индексов, где произошла склейка двух сплайнов
        List<int> mergedKnotIndices = new List<int>();

        foreach (var container in splinesToMerge)
        {
            if (container == null) continue;

            if (container.Splines.Count <= splineIndex)
            {
                Debug.LogWarning($"В контейнере {container.name} нет сплайна с индексом {splineIndex}. Пропускаем.");
                continue;
            }

            Spline sourceSpline = container.Splines[splineIndex];

            for (int i = 0; i < sourceSpline.Count; i++)
            {
                BezierKnot knot = sourceSpline[i];

                // Переводим позицию и тангенты точки из локального пространства исходного сплайна в мировое
                Vector3 worldPos = container.transform.TransformPoint(knot.Position);
                Vector3 worldTangentIn = container.transform.TransformDirection(knot.TangentIn);
                Vector3 worldTangentOut = container.transform.TransformDirection(knot.TangentOut);

                // Переводим мировые координаты в локальное пространство нашего целевого сплайна
                Vector3 localPos = transform.InverseTransformPoint(worldPos);
                Vector3 localTangentIn = transform.InverseTransformDirection(worldTangentIn);
                Vector3 localTangentOut = transform.InverseTransformDirection(worldTangentOut);

                // Создаем новую точку для целевого сплайна
                BezierKnot newKnot = new BezierKnot
                {
                    Position = localPos,
                    TangentIn = localTangentIn,
                    TangentOut = localTangentOut,
                    Rotation = knot.Rotation
                };

                if (isFirstKnot)
                {
                    targetSpline.Add(newKnot);
                    lastKnotPosition = localPos;
                    isFirstKnot = false;
                }
                else
                {
                    // Проверяем, совпадает ли первая точка нового сплайна с последней добавленной точкой
                    if (i == 0 && Vector3.Distance(localPos, lastKnotPosition) <= mergeTolerance)
                    {
                        int lastKnotIndex = targetSpline.Count - 1;
                        BezierKnot lastKnot = targetSpline[lastKnotIndex];

                        // Соединяем геометрию: берем TangentOut от нового сплайна
                        lastKnot.TangentOut = localTangentOut;
                        targetSpline[lastKnotIndex] = lastKnot;

                        // Запоминаем индекс этой точки, чтобы сгладить её после формирования сплайна
                        mergedKnotIndices.Add(lastKnotIndex);

                        // Пропускаем дублирующуюся точку
                        continue;
                    }

                    targetSpline.Add(newKnot);
                    lastKnotPosition = localPos;
                }
            }
        }

        // Применяем Continuous-сглаживание в местах склейки
        foreach (int index in mergedKnotIndices)
        {
            // Вызываем SetTangentMode непосредственно у экземпляра Spline
            targetSpline.SetTangentMode(index, TangentMode.Continuous);
        }

        // Корректно уведомляем Unity об изменении данных сплайна
#if UNITY_EDITOR
        EditorUtility.SetDirty(targetContainer);
#endif

        Debug.Log("Сплайны успешно объединены и сглажены в местах стыков!", this);
    }
}
