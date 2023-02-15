using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrganImage : MonoBehaviour
{

    [SerializeField]
    private Image organImage;

    public void FounImage() {

        organImage.color = Color.white;
    }
}
