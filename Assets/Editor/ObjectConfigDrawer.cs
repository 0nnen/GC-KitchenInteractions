using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectConfig))]
public class ObjectConfigDrawer : PropertyDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
    private const float VerticalSpacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Initialisation
        Rect currentRect = new Rect(position.x, position.y, position.width, LineHeight);
        var prefabProp = property.FindPropertyRelative("prefab");
        var ingredientDataProp = property.FindPropertyRelative("ingredientData");
        var canReceiveChildrenProp = property.FindPropertyRelative("canReceiveChildren");
        var hasDoorProp = property.FindPropertyRelative("hasDoor");
        var isMovableProp = property.FindPropertyRelative("isMovable");
        var dropZoneColliderProp = property.FindPropertyRelative("dropZoneCollider");
        var doorTransformProp = property.FindPropertyRelative("doorTransform");
        var rotateDoorOnXProp = property.FindPropertyRelative("rotateDoorOnX");
        var rotateDoorOnYProp = property.FindPropertyRelative("rotateDoorOnY");
        var invertDoorRotationProp = property.FindPropertyRelative("invertDoorRotation");
        var doorRotationLimitProp = property.FindPropertyRelative("doorRotationLimit");

        // Dessiner les champs principaux
        EditorGUI.PropertyField(currentRect, prefabProp, new GUIContent("Prefab"));
        currentRect.y += LineHeight + VerticalSpacing;

        EditorGUI.PropertyField(currentRect, ingredientDataProp, new GUIContent("Ingredient Data"));
        currentRect.y += LineHeight + VerticalSpacing;

        EditorGUI.PropertyField(currentRect, isMovableProp, new GUIContent("Is Movable"));
        currentRect.y += LineHeight + VerticalSpacing;

        // Champs conditionnels : CanReceiveChildren
        EditorGUI.PropertyField(currentRect, canReceiveChildrenProp, new GUIContent("Can Receive Children"));
        currentRect.y += LineHeight + VerticalSpacing;

        if (canReceiveChildrenProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(currentRect, dropZoneColliderProp, new GUIContent("Drop Zone Collider"));
            currentRect.y += LineHeight + VerticalSpacing;
            EditorGUI.indentLevel--;
        }

        // Champs conditionnels : HasDoor
        EditorGUI.PropertyField(currentRect, hasDoorProp, new GUIContent("Has Door"));
        currentRect.y += LineHeight + VerticalSpacing;

        if (hasDoorProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(currentRect, doorTransformProp, new GUIContent("Door Transform"));
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, rotateDoorOnXProp, new GUIContent("Rotate Door on X"));
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, rotateDoorOnYProp, new GUIContent("Rotate Door on Y"));
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, invertDoorRotationProp, new GUIContent("Invert Door Rotation"));
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, doorRotationLimitProp, new GUIContent("Door Rotation Limit"));
            currentRect.y += LineHeight + VerticalSpacing;
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = LineHeight + VerticalSpacing * 30; // Base fields
        if (property.FindPropertyRelative("canReceiveChildren").boolValue)
            height += LineHeight + VerticalSpacing;

        if (property.FindPropertyRelative("hasDoor").boolValue)
            height += (LineHeight + VerticalSpacing) * 5;

        return height;
    }
}
