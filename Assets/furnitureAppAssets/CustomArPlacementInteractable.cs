using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class CustomArPlacementInteractable : ARPlacementInteractable 
{
    private List<RaycastResult> raycastHits = new List<RaycastResult>();

    protected override bool CanStartManipulationForGesture(TapGesture gesture)
    {

        if (gestureInteractor.interactablesSelected.Count >0)
        {
            return false;
        }


        PointerEventData eventData = new PointerEventData(EventSystem.current);

        eventData.position = gesture.startPosition;
        EventSystem.current.RaycastAll(eventData, raycastHits);


        if(raycastHits.Count > 0){

            return false;
        }


        return base.CanStartManipulationForGesture(gesture);


    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
