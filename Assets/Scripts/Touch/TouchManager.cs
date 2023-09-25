using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchManager : MonoBehaviour{

    [SerializeField] private GameManager gameManager;

    public int minimumSwipeOffset = 250;

    private PlayerInput playerInput;
    private InputAction touchPressedAction;

    void Awake(){
        playerInput = GetComponent<PlayerInput>();
        touchPressedAction = playerInput.actions.FindAction("TouchPress");
    }

    private void OnEnable() {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        touchPressedAction.performed += TouchPressed;
    }

    private void OnDisable() {
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
        touchPressedAction.performed -= TouchPressed;
    }

    private void TouchPressed(InputAction.CallbackContext context) => StartCoroutine(TouchCoroutine());


    private IEnumerator TouchCoroutine() {
        yield return null;
        while (Touch.activeTouches.Count != 0) {
            //print(Touch.activeTouches[0].screenPosition +" ve "+ Touch.activeTouches[0].startScreenPosition);
            float surplusX = Touch.activeTouches[0].screenPosition.x - Touch.activeTouches[0].startScreenPosition.x;
            float surplusY = Touch.activeTouches[0].screenPosition.y - Touch.activeTouches[0].startScreenPosition.y;

            if (surplusX > minimumSwipeOffset) {
                gameManager.Swipe(Vector2.right);
                break;
            }
            else if (surplusX < -minimumSwipeOffset) {
                gameManager.Swipe(Vector2.left);
                break;
            }
            else if (surplusY > minimumSwipeOffset) {
                gameManager.Swipe(Vector2.up);
                break;
            }
            else if (surplusY < -minimumSwipeOffset) {
                gameManager.Swipe(Vector2.down);
                break;
            }
            yield return null;
        }
    }

}
