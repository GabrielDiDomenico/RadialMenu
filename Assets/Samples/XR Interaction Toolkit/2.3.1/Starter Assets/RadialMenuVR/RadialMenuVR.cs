using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;


namespace VR
{
    public class RadialMenuVR : MonoBehaviour
    {
        [SerializeField] float Radius = 5;
        [SerializeField] List<RadialMenuEntry> Entries;
        [SerializeField] RectTransform rect;
        [SerializeField] Transform handPos;
        [SerializeField] Transform leftHandPos;
        [SerializeField] GameObject menuModeCanvas;
        [SerializeField] Vector3 menuPosOffset;
        [SerializeField] GameObject hand;
        [SerializeField] InputActionReference toggleMenuOpenRef;
        [SerializeField] InputActionReference holdRotateMenuRef;
        [SerializeField] InputActionReference toggleMenuTypeRef;
        [SerializeField] XRInteractorLineVisual rayColor;
        [SerializeField] GameObject menuArrow;

        [SerializeField] GameObject button;
        List<GameObject> Buttons;
        Dictionary<float, string> ButtonsAngles;
        bool isOpen;
        bool isRotationActive;
        bool canRotateMenu;
        float lastAngle;
        bool isInAnimation;
        float animationSpeed;
        int menuId;
        float buttonScale;
        List<Vector3> currButtonsPositions;
        GameObject buttonToPaint;
        matrixClass matrixBezier;

        
        public void Awake()
        {
            toggleMenuOpenRef.action.started += ToggleMenuOpen;
            holdRotateMenuRef.action.started += HoldRotateMenu;
            holdRotateMenuRef.action.canceled += HoldRotateMenu;
            toggleMenuTypeRef.action.started += ToggleMenuType;
        }

        public void Start()
        {
            Buttons = new List<GameObject>();
            ButtonsAngles = new Dictionary<float, string>();
            currButtonsPositions = new List<Vector3>();
            rect.SetPositionAndRotation(handPos.position + menuPosOffset, handPos.rotation);
            isOpen = false;
            isRotationActive = false;
            canRotateMenu = true;
            lastAngle = 0;
            menuId = 0;
            matrixBezier = new matrixClass(4);
            
        }

        public IEnumerator MoveAnchoPos(GameObject obj, Vector3 pos, bool dir, float vel, bool canFinishAnim, bool destroyMenu)
        {

            float objX = obj.transform.localPosition.x;
            float objY = obj.transform.localPosition.y;
            float posX = pos.x;
            float posY = pos.y;

            Vector3 p1 = obj.transform.localPosition;
            Vector3 p2;
            if (!dir)
            {
                p2 = new Vector3((((1 / 2) * (posX + objX)) - ((float)(Math.Sqrt(3) / 2) * (posY - objY))) * 3,
                                     (((1 / 2) * (posY + objY)) + ((float)(Math.Sqrt(3) / 2) * (posX - objX))) * 3, 0);
            }
            else
            {
                p2 = new Vector3((((1 / 2) * (posX + objX)) + ((float)(Math.Sqrt(3) / 2) * (posY - objY))) * 3,
                                     (((1 / 2) * (posY + objY)) - ((float)(Math.Sqrt(3) / 2) * (posX - objX))) * 3, 0);
            }

            float t = 0.0f;
            while (t < 1.0f)
            {
                float pX = (1 - t) * (1 - t) * p1.x + 2 * (1 - t) * t * p2.x + t * t * pos.x;
                float pY = (1 - t) * (1 - t) * p1.y + 2 * (1 - t) * t * p2.y + t * t * pos.y;
                Debug.Log("X: " + pX + "Y: " + pY);
                obj.transform.SetLocalPositionAndRotation(new Vector3(pX, pY, 0), new Quaternion(0, 0, 0, 0));
                t += Time.deltaTime * vel;

                yield return null;
            }

            obj.transform.SetLocalPositionAndRotation(pos, new Quaternion(0, 0, 0, 0));
            if (canFinishAnim)
                AnimationFinished();
            if (destroyMenu)
                DestroyMenu();
        }

        public IEnumerator Scale(GameObject obj, float upScale, float duration, bool canFinishAnim)
        {
            Vector3 initialScale = obj.transform.localScale;

            for (float time = 0; time < duration * 2; time += Time.deltaTime * 2)
            {
                obj.transform.localScale = Vector3.Lerp(initialScale, new Vector3(upScale, upScale, 1), time);
                yield return null;
            }
            if (canFinishAnim)
                AnimationFinished();
        }

        public void Update()
        {
            menuModeCanvas.transform.position = leftHandPos.position;
            menuModeCanvas.transform.rotation = leftHandPos.rotation;
            switch (menuId)
            {
                case 0:
                    menuModeCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "By Angle";
                    break;
                case 1:
                    menuModeCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Highlight";
                    break;
                case 2:
                    menuModeCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Continuous";
                    break;
                case 3:
                    menuModeCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "One step";
                    break;
            }
            
            if (!isRotationActive)
            {
                transform.position = hand.transform.position;
                menuArrow.transform.position = transform.position;
                transform.rotation = hand.transform.rotation;
                menuArrow.transform.rotation = transform.rotation;
            }
            else
            {
                transform.position = hand.transform.position;
                menuArrow.transform.position = transform.position;
                CurrentMenuRender();

            }

            if ((hand.transform.rotation.eulerAngles.z > 0 && hand.transform.rotation.eulerAngles.z < 4) || (hand.transform.rotation.eulerAngles.z > -6 && hand.transform.rotation.eulerAngles.z < 0))
            {
                canRotateMenu = true;
            }
        }

        public void OnDestroy()
        {
            toggleMenuOpenRef.action.started -= ToggleMenuOpen;
            holdRotateMenuRef.action.started -= HoldRotateMenu;
            holdRotateMenuRef.action.canceled -= HoldRotateMenu;
            toggleMenuTypeRef.action.started -= ToggleMenuType;
        }

        public void CurrentMenuRender()
        {
            if(menuId == 0)
            {
                    animationSpeed = .1f;
                    RotateMenuByAngle(hand.transform.rotation.eulerAngles.z, false);
                
            }
            else if(menuId == 1)
            {
                if ((hand.transform.rotation.eulerAngles.z > 2 && hand.transform.rotation.eulerAngles.z < 358))
                {
                    animationSpeed = .0001f;
                    RotateMenuByAngle(hand.transform.rotation.eulerAngles.z, true);
                }
            }
            else if (menuId == 2)
            {
                if ((hand.transform.rotation.eulerAngles.z > 10 && hand.transform.rotation.eulerAngles.z < 350) && !isInAnimation)
                {
                    animationSpeed = .4f;
                    RotateMenuByDirection(hand.transform.rotation.eulerAngles.z);
                }
            }
            else if(menuId == 3)
            {
                animationSpeed = .4f;
                if ((hand.transform.rotation.eulerAngles.z > 10 && hand.transform.rotation.eulerAngles.z < 350) && !isInAnimation)
                {
                    RotateMenuOneByOne(hand.transform.rotation.eulerAngles.z);
                }
            }
        }

        public void ToggleMenuType(InputAction.CallbackContext context)
        {
            if(menuId == 3)
            {
                menuId=0;
                return;
            }
            menuId++;
        }

        private void ToggleMenuOpen(InputAction.CallbackContext context)
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void HoldRotateMenu(InputAction.CallbackContext context)
        {
            if (!isOpen)
            {
                return;
            }
            List<Image> icons = Buttons[0].GetComponentsInChildren<Image>(true).ToList<Image>();
            if (isRotationActive)
            {
                if (menuId != 0)
                    icons.First(s => s.name == "Seta").gameObject.SetActive(false);
                else
                    menuArrow.SetActive(false);
                isRotationActive = false;
                lastAngle = 0;

                if (menuId != 0)
                    Buttons[0].GetComponent<Button>().onClick.Invoke();
                else
                    buttonToPaint.GetComponent<Button>().onClick.Invoke();
            }
            else
            {
                if(menuId!=0)
                    icons.First(s => s.name == "Seta").gameObject.SetActive(true);
                else
                    menuArrow.SetActive(true);
                isRotationActive = true;
                
            }
        }

        private void RotateMenuOneByOne(float handAngle)
        {
            //Checa se pode rodar
            if (!canRotateMenu) return;
            RotateMenuByDirection(handAngle);
            canRotateMenu = false;
        }
        private void AnimationFinished()
        {
            isInAnimation = false;
        }
        private void updateAllButtonsPos()
        {
            currButtonsPositions.Clear();
            foreach (var b in Buttons)
            {
                currButtonsPositions.Add(b.gameObject.transform.localPosition);
            }
        }

        private void RotateButtonsByAngleLeft(bool highlightMode)
        {
            Debug.Log("Entro esq");
            //Rotaciona
            RectTransform currButtonToRotate;
            updateAllButtonsPos();

            //tira seta
            List<Image> icons = Buttons[0].GetComponentsInChildren<Image>(true).ToList<Image>();
            icons.First(s => s.name == "Seta").gameObject.SetActive(false);
            //Ordena com a nova posição

            int lastIndex = Buttons.Count - 1;
            var lastButton = Buttons[lastIndex];
            var lastPos = currButtonsPositions[lastIndex];
            Buttons.RemoveAt(lastIndex);
            Buttons.Insert(0, lastButton);
            currButtonsPositions.RemoveAt(lastIndex);
            currButtonsPositions.Insert(0, lastPos);
            icons = Buttons[0].GetComponentsInChildren<Image>(true).ToList<Image>();
            icons.First(s => s.name == "Seta").gameObject.SetActive(true);
            //Bota a seta
            if (!highlightMode)
            {
               
                isInAnimation = true;
                for (int i = 0; i < Buttons.Count - 1; i++)
                {
                    StartCoroutine(MoveAnchoPos(Buttons[i], currButtonsPositions[i + 1], false, 2.0f, false,false));
                    StartCoroutine(Scale(Buttons[i], buttonScale, 0.6f, false));
                }
                StartCoroutine(MoveAnchoPos(Buttons[Buttons.Count - 1], currButtonsPositions[0], false, 2.0f, false,false));
                StartCoroutine(Scale(Buttons[Buttons.Count - 1], buttonScale, 0.6f, false));
                StartCoroutine(Scale(Buttons[0], buttonScale * 1.3f, 0.6f, true));

            }
            updateAllButtonsPos();
        }

        private void RotateButtonsByAngleRight(bool highlightMode)
        {
            Debug.Log("Entro dir");
            //Rotaciona
            RectTransform currButtonToRotate;
            updateAllButtonsPos();

            //tira seta
            List<Image> icons = Buttons[0].GetComponentsInChildren<Image>(true).ToList<Image>();
            icons.First(s => s.name == "Seta").gameObject.SetActive(false);
            //Ordena com a nova posição
            var auxButton = Buttons[0];
            var auxPos = currButtonsPositions[0];
            Buttons.Add(auxButton);
            Buttons.RemoveAt(0);
            currButtonsPositions.Add(auxPos);
            currButtonsPositions.RemoveAt(0);
            icons = Buttons[0].GetComponentsInChildren<Image>(true).ToList<Image>();
            icons.First(s => s.name == "Seta").gameObject.SetActive(true);
            //Bota a seta
            if (!highlightMode)
            {
                
                isInAnimation = true;
                for (int i = Buttons.Count - 1; i > 0; i--)
                {
                    StartCoroutine(MoveAnchoPos(Buttons[i], currButtonsPositions[i - 1], true, 2.0f, false, false));
                    StartCoroutine(Scale(Buttons[i], buttonScale, 0.6f, false));
                }
                StartCoroutine(MoveAnchoPos(Buttons[0], currButtonsPositions[Buttons.Count - 1], true, 2.0f, false,false));
                StartCoroutine(Scale(Buttons[0], buttonScale * 1.3f, 0.6f, true));

            }
            updateAllButtonsPos();
        }

        private GameObject FindClosestButton()
        {
            GameObject closestButton = Buttons[0];
            float closestDistance = float.MaxValue;
            GameObject arrow = menuArrow.GetComponentInChildren<Image>().gameObject;

            foreach (GameObject b in Buttons)
            {
                float distance = Vector3.Distance(arrow.transform.position, b.transform.position);
                b.GetComponent<RectTransform>().DOScale(new Vector3(buttonScale, buttonScale, 1f), .3f).SetEase(Ease.OutQuad);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestButton = b;
                   
                }
            }
            closestButton.GetComponent<RectTransform>().DOScale(new Vector3(buttonScale + .2f, buttonScale + .2f, 1f), .3f).SetEase(Ease.OutQuad);
            return closestButton;
        }

        private void RotateMenuByAngle(float handAngle, bool highlightMode)
        {
            
            if (!highlightMode)
            {
                float adjustedAngle = handAngle * 2;

                if (adjustedAngle > 360)
                    adjustedAngle -= 360;
                else if (adjustedAngle < 0)
                    adjustedAngle += 360;

                float currentAngle = transform.rotation.eulerAngles.z;

                float angleDiff = adjustedAngle - currentAngle;
                float shortestAngleDiff = Mathf.DeltaAngle(currentAngle, adjustedAngle);

                float direction = Mathf.Sign(shortestAngleDiff);


                if (shortestAngleDiff != 0)
                {
                    transform.Rotate(new Vector3(0, 0, 1), direction * Mathf.Min(Mathf.Abs(angleDiff), 3.0f));
                    foreach (GameObject b in Buttons)
                    {
                        b.transform.Rotate(new Vector3(0, 0, 1), -direction * Mathf.Min(Mathf.Abs(angleDiff), 3.0f));

                    }
                }
                buttonToPaint = FindClosestButton();
                Debug.Log("Z angle: " + transform.rotation.eulerAngles.z + "Hand angle: " + handAngle * 2);
            }
            else
            {
                string nameButtonDest = findButtonNameByAngle(handAngle);
                while (Buttons[0].GetComponentInChildren<TextMeshProUGUI>().text != nameButtonDest)
                {

                    if (handAngle < lastAngle)
                    {
                        if (lastAngle - handAngle <= 180)
                        {

                            RotateButtonsByAngleRight(highlightMode);
                        }
                        else
                        {

                            RotateButtonsByAngleLeft(highlightMode);
                        }
                    }
                    else if (handAngle > lastAngle)
                    {
                        if (handAngle - lastAngle <= 180)
                        {

                            RotateButtonsByAngleLeft(highlightMode);
                        }
                        else
                        {

                            RotateButtonsByAngleRight(highlightMode);
                        }
                    }

                }
                lastAngle = handAngle;
            }
            
        }

        private void RotateMenuByDirection(float handAngle)
        {
            
            if (handAngle > 180)
            {
                RotateButtonsByAngleRight(false);
            }
            else if (handAngle < 180)
            {
               
                RotateButtonsByAngleLeft(false);
            }
            
            lastAngle = handAngle;
        }

        private string findButtonNameByAngle(float handAngle)
        {
            var allKeys = ButtonsAngles.Keys;
            var adjustedAngle = handAngle * 2;

            if (adjustedAngle > 360)
                adjustedAngle -= 360;
            else if(adjustedAngle<0)
                adjustedAngle += 360;


            float closestDifference = float.MaxValue;
            string closestButtonName = string.Empty;

            foreach (float key in allKeys)
            {
                float angleDifference = Mathf.Abs(adjustedAngle - key);
                if (angleDifference < closestDifference)
                {
                    closestDifference = angleDifference;
                    closestButtonName = ButtonsAngles[key];
                }
            }

            foreach(GameObject b in Buttons)
            {
                if(closestButtonName == b.GetComponentInChildren<TextMeshProUGUI>().text)
                {
                    b.GetComponent<RectTransform>().DOScale(new Vector3(buttonScale + .2f, buttonScale + .2f, 1f), .3f).SetEase(Ease.OutQuad);
                }
                else
                {
                    b.GetComponent<RectTransform>().DOScale(new Vector3(buttonScale, buttonScale, 1f), .3f).SetEase(Ease.OutQuad);
                }
            }

            return closestButtonName;
        }

        public void SpawnEntries()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                GameObject instantiatedButton = Instantiate(button, transform);
                instantiatedButton.GetComponent<Button>().onClick.AddListener(Entries[i].uEvent.Invoke);
                instantiatedButton.GetComponentInChildren<TextMeshProUGUI>().text = Entries[i].label;
                List<Image> icons = instantiatedButton.GetComponentsInChildren<Image>().ToList<Image>();
                icons.First(s => s.name == "Icone").sprite = Entries[i].icon;
                Buttons.Add(instantiatedButton);
            }

        }
        public void ChangeRayColorRed()
        {
            Color c = new Color(1, 0, 0, 1);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }

        public void ChangeRayColorYellow()
        {
            Color c = new Color(1, 1, 0);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }

        public void ChangeRayColorBlue()
        {
            Color c = new Color(0, 0, 1);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorGreen()
        {
            Color c = new Color(0, 1, 0);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorPurple()
        {
            Color c = new Color(0.6f, 0, 1);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorOrange()
        {
            Color c = new Color(1, 0.6f, 0);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorWhite()
        {
            Color c = new Color(1, 1, 1);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorBlack()
        {
            Color c = new Color(0, 0, 0);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorBabyBlue()
        {
            Color c = new Color(0.2901961f, 0.5254902f, 0.909804f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorCyan()
        {
            Color c = new Color(0, 1, 1);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorKakhi()
        {
            Color c = new Color(0.9411765f, 0.9019608f, 0.5490196f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorIndigo()
        {
            Color c = new Color(0.2941177f, 0, 0.509804f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorSalmon()
        {
            Color c = new Color(1, 0.627451f, 0.4784314f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorDarkGreen()
        {
            Color c = new Color(0, 0.3921569f, 0);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorBrown()
        {
            Color c = new Color(0.4705883f, 0.2470588f, 0.007843138f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorPink()
        {
            Color c = new Color(1, 0.4117647f, 0.7058824f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
        public void ChangeRayColorOliverGreen()
        {
            Color c = new Color(0.4196079f, 0.5568628f, 0.1372549f);
            rayColor.validColorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            rayColor.validColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(c, 0.0f), new GradientColorKey(c, 1.0f) };
            Close();
        }
       
        public void Open()
        {
            SpawnEntries();
            Rearrange();
            isOpen = true;
            Buttons[0].SetActive(true);
            Buttons[0].GetComponent<Button>().Select();
        }

        public void Close()
        {
            for (int i = 0; i < Buttons.Count-1; i++)
            {

                GameObject currButton = Buttons[i].gameObject;
                StartCoroutine(MoveAnchoPos(Buttons[i], Vector3.zero, false, 1f, true,false));
                                                                                             
 
                                                                                            
            }
            StartCoroutine(MoveAnchoPos(Buttons[Buttons.Count - 1], Vector3.zero, false, 1f, true,true));

            Buttons.Clear();
            ButtonsAngles.Clear();
            isOpen = false;
        }
        public void DestroyMenu()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void Rearrange()
        {
            float radiansOfSeparation = (Mathf.PI * 2) / Entries.Count;
            float angle_increment = 360 / Buttons.Count;


            float minScale = 0.1f; // Minimum scale factor
            float maxScale = 1f; // Maximum scale factor
            float x, y;

            for (int i = 0; i < Buttons.Count; i++)
            {
                if(i == 0)
                {
                    ButtonsAngles.Add(angle_increment-5, Buttons[(Buttons.Count - 1) - i].GetComponentInChildren<TextMeshProUGUI>().text);
                }
                else
                {
                    ButtonsAngles.Add(angle_increment * i, Buttons[(Buttons.Count - 1) - i].GetComponentInChildren<TextMeshProUGUI>().text);
                }
                
                
                
                x = Mathf.Sin(radiansOfSeparation * i) * Radius;
                y = Mathf.Cos(radiansOfSeparation * i) * Radius;
              
                // Calculate the distance between buttons (assuming a circular shape)
                float distance = Radius * radiansOfSeparation;

                // Calculate the scale factor based on the distance
                buttonScale = Mathf.Lerp(minScale, maxScale, distance / Radius);

                StartCoroutine(MoveAnchoPos(Buttons[i], new Vector3(x, y, 0), false, 1f, false,false));
                StartCoroutine(Scale(Buttons[i], buttonScale, 0.6f, false));
                
            }
        }
    }
}

