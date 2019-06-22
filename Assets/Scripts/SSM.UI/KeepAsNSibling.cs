using UnityEngine;

public class KeepAsNSibling : MonoBehaviour
{
    public int n;

    void Update()
    {
        if (transform.GetSiblingIndex() != n)
        {
            transform.SetSiblingIndex(n);
        }
    }
}