using UnityEngine;

public class KeepAsLastSibling : MonoBehaviour
{
    void Update()
    {
        if (transform.GetSiblingIndex() != transform.parent.childCount - 1)
        {
            transform.SetAsLastSibling();
        }
    }
}