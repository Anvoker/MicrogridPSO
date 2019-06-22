using UnityEditor;

namespace EditorAdditions
{
    public class ClassDrawer<T> : PropertyDrawer where T : class, new()
    {
        protected T instance;
        protected virtual void Initialize(SerializedProperty prop)
        {
            object propertyObject;
            string[] path = prop.propertyPath.Split('.');
            propertyObject = prop.serializedObject.targetObject;
            foreach (string pathNode in path)
            {
                propertyObject = propertyObject.GetType().GetField(pathNode).GetValue(propertyObject);
            }

            instance = (T)propertyObject;
        }
    }
}