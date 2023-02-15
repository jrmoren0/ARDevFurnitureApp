using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSelecter : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
   
    // Update is called once per frame
    void Update()
    {
        Vector3 middleofScrreen = _camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
        Vector3 middleOfMagnifiingGlass = transform.position;
        Vector3 directionToGlass = (middleOfMagnifiingGlass - middleofScrreen).normalized;

        if(!Physics.Raycast(transform.position, directionToGlass,out RaycastHit hit))
            return;

        OrganImage organ = hit.collider.GetComponent<OrganImage>();
            if (organ == null)
                return;

        organ.FounImage();

        


    }
}
