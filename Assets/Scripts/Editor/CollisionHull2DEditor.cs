using UnityEditor;

[CustomEditor(typeof(CollisionHull2D))]
public class CollisionHull2DEditor : Editor
{
    private SerializedProperty typeProp;
    private SerializedProperty centerProp;
    private SerializedProperty sizeProp;
    private SerializedProperty radiusProp;

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("hullData.Type");
        centerProp = serializedObject.FindProperty("hullData.Center");
        sizeProp = serializedObject.FindProperty("hullData.Size");
        radiusProp = serializedObject.FindProperty("hullData.Radius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(typeProp);

        switch((CollisionHull2D.Collider2DType)typeProp.enumValueIndex)
        {
            case CollisionHull2D.Collider2DType.BOX:
                EditorGUILayout.PropertyField(centerProp);
                EditorGUILayout.PropertyField(sizeProp);
                break;

            case CollisionHull2D.Collider2DType.CIRCLE:
                EditorGUILayout.PropertyField(centerProp);
                EditorGUILayout.PropertyField(radiusProp);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}