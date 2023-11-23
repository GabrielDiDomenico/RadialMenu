using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasHandler : MonoBehaviour
{

    [SerializeField] private XRRayInteractor rightHandRay;
    [SerializeField] private XRInteractorLineVisual colorToPaint;
    [SerializeField] private GameObject brasilStatesObj;
    [SerializeField] private InputActionReference clickToPaint;
    private RaycastResult currCast;
    // Start is called before the first frame update

    private void Awake()
    {
        clickToPaint.action.started += PaintCurrentSelectedState;
    }
    void Start()
    {
        
        List<Image> icons = brasilStatesObj.GetComponentsInChildren<Image>().ToList();
        foreach(Image img in icons)
        {
            Debug.Log(img.name);
            img.alphaHitTestMinimumThreshold = .0001f;
        }
    }

    // Update is called once per frame
    void Update()
    {

        rightHandRay.TryGetCurrentUIRaycastResult(out currCast);

    }

    private void OnDestroy()
    {
        clickToPaint.action.started -= PaintCurrentSelectedState;
    }

    private void PaintCurrentSelectedState(InputAction.CallbackContext context)
    {
        currCast.gameObject.GetComponent<Image>().color = colorToPaint.validColorGradient.colorKeys[0].color;
    }
}
