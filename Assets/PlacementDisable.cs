using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class PlacementDisable : MonoBehaviour, IPointerDownHandler,IPointerUpHandler// required interface when using the OnPointerDown method.
{

    public GameObject ARPlacement;
    //Do this when the mouse is clicked over the selectable object this script is attached to.
    public void OnPointerDown(PointerEventData eventData)
    {
        ARPlacement.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ARPlacement.SetActive(true);
    }
}