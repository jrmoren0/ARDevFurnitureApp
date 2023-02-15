using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class DeleteSelectedObject : MonoBehaviour
{

    [SerializeField]
    private Button deleteButton;


    private GameObject currenSelected;



    // Start is called before the first frame update
    void Start()
    {
        deleteButton.onClick.AddListener(DeleteSelected);
    }

   public void SelectEnter(SelectEnterEventArgs args)
    {
        currenSelected = args.interactableObject.transform.gameObject;
        deleteButton.interactable = true;
    }


    public void SelectExit(SelectExitEventArgs args)
    {
        deleteButton.interactable = false;
    }



    public void DeleteSelected() {

        Destroy(currenSelected);

    }


}
