using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;

public class Cell : MonoBehaviour
{
    // Main
    public Sprite sprite;
    //public UnityEvent trigger; // Зачем, если в UI тоже есть своего рода ивенты?

    // Start
    private Image subImage;
    void Start()
    {
        Transform subTransform = gameObject.transform.Find("Image");

        // Update
        subImage = subTransform.GetComponent<Image>();
        subImage.sprite = sprite;
        subImage.SetNativeSize();
    }

    // Pressed
    public void ButtonPressed()
    {
        Controller.i.CheckCell(gameObject, sprite);
    }
}
