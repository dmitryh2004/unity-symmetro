using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

[CustomEditor(typeof(SplineContainer))]
public class SplineInverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Отрисовываем стандартный инспектор SplineContainer
        DrawDefaultInspector();

        SplineContainer container = (SplineContainer)target;

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f);
        
        if (GUILayout.Button("Развернуть сплайн (Инвертировать)", GUILayout.Height(30)))
        {
            // Регистрируем операцию Undo, чтобы изменения можно было отменить (Ctrl+Z)
            Undo.RegisterCompleteObjectUndo(container, "Invert Spline Direction");

            // Инвертируем основной сплайн (по умолчанию индекс 0)
            // Если у вас много сплайнов в одном контейнере, можно пустить цикл по container.Splines
            if (container.Spline != null)
            {
                InvertSpline(container.Spline);
                
                // Уведомляем Unity об изменении данных сплайна для обновления сцены
                // container.UpdateSpline();
                EditorUtility.SetDirty(container);
                
                Debug.Log("Сплайн успешно развернут на 180 градусов!");
            }
        }
    }

    private void InvertSpline(Spline spline)
    {
        int knotCount = spline.Count;
        if (knotCount < 2) return;

        // Создаем временный список для новых инвертированных узлов
        List<BezierKnot> invertedKnots = new List<BezierKnot>(knotCount);

        // Проходим по узлам с конца в начало
        for (int i = knotCount - 1; i >= 0; i--)
        {
            BezierKnot originalKnot = spline[i];
            BezierKnot invertedKnot = new BezierKnot();

            // Позиция узла остается прежней
            invertedKnot.Position = originalKnot.Position;

            // Разворачиваем касательные на 180 градусов (умножаем вектор на -1)
            // Также меняем местами Входящую (TangentIn) и Исходящую (TangentOut) касательные,
            // так как при движении в обратную сторону они меняются ролями.
            invertedKnot.TangentIn = -originalKnot.TangentOut;
            invertedKnot.TangentOut = -originalKnot.TangentIn;

            // Вращение узла (Rotation) инвертируем, поворачивая локальную ось Z назад.
            // Это автоматически развернет направление EvaluateTangent и EvaluateUpVector.
            invertedKnot.Rotation = originalKnot.Rotation * Quaternion.Euler(0, 180, 0);

            invertedKnots.Add(invertedKnot);
        }

        // Записываем инвертированные узлы обратно в сплайн
        spline.Knots = invertedKnots;
    }
}
