using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class FurnitureManager : MonoBehaviour
{
    [SerializeField]
    private FurnitureData[] furnitureData;


    [SerializeField]
    private Transform contentTransform;


    //  [SerializeField]
    // private GameObject UIPrefab;

    [SerializeField]
    private FurnitureMenuOptions UIPrefab;


    [SerializeField]
    private ARPlacementInteractable aRPlacementInteractable;


    private FurnitureMenuOptions currenmenuOption;




    private void Start()
    {

        foreach(FurnitureData data in furnitureData)
        {

           FurnitureMenuOptions menuOption =  Instantiate(UIPrefab, contentTransform);
            menuOption.nameText.text = data.furnitureName;
            menuOption.iconImage.texture = data.furnitureIcon;

            menuOption.button.onClick.AddListener(() => SelectFurniture(menuOption, data)); 



          
        }



    }


    private void SelectFurniture(FurnitureMenuOptions newMenuOption, FurnitureData furnitureData)
    {
        if(currenmenuOption != null)
        {
            currenmenuOption.backgroundImage.color = newMenuOption.backgroundImage.color;
        }


        currenmenuOption = newMenuOption;

        aRPlacementInteractable.placementPrefab = furnitureData.furniturePrefab;


        newMenuOption.backgroundImage.color = Color.yellow;


    }


}
