using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailSplineMerger))]
public class RailSplineMergerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Отрисовываем стандартный инспектор (список сплайнов и погрешность)
        DrawDefaultInspector();

        RailSplineMerger myScript = (RailSplineMerger)target;

        GUILayout.Space(15);

        // Кнопка для генерации
        if (GUILayout.Button("Сгенерировать сплайн", GUILayout.Height(35)))
        {
            // Фиксируем изменения для возможности отмены (Undo)
            Undo.RegisterCompleteObjectUndo(myScript.gameObject, "Merge Splines");

            myScript.GenerateMergedSpline();
        }
    }
}
