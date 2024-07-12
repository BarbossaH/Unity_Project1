using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed;
    private float inputX;
    private float inputY;

    private Animator[] animators;
    private Vector2 movementInput;
    private bool isMoving;

    private bool canInput;

    private float mouseX, mouseY;
    // bool isUsingTool;
    private void Awake()
    {
        canInput = true;
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
    }

    private void OnEnable()
    {
        NotifyCenter<SceneEvent, Vector3, bool>.notifyCenter += OnMovePlayer;

        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSceneChange;
        NotifyCenter<SceneEvent, Vector3, ItemDetails>.notifyCenter += OnInputEvent;

    }

    private void OnInputEvent(SceneEvent sceneEvent, Vector3 mouseWorldPos, ItemDetails details)
    {
        if (sceneEvent == SceneEvent.MouseClickEvent)
        {
            //TODO : play the animation
            if (details.itemType != ItemType.Seed && details.itemType != ItemType.Commodity && details.itemType != ItemType.Furniture)
            {
                mouseX = mouseWorldPos.x - transform.position.x;
                mouseY = mouseWorldPos.y - transform.position.y;

                //哪个方向离的更远，就优先判断哪个方向，这样就可以判断工具的朝向
                if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
                {
                    mouseY = 0;
                }
                else
                {
                    mouseX = 0;
                }
                StartCoroutine(UseToolRoutine(mouseWorldPos, details));
            }
            else
            {
                //then notify the scene to response
                NotifyCenter<SceneEvent, Vector3, ItemDetails>.NotifyObservers(SceneEvent.AfterPlayerAnimation, mouseWorldPos, details);
            }
        }
    }
    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails details)
    {
        // isUsingTool = true;
        canInput = false;
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger("UseTool");
            //这里只是把人物转个身
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);
        NotifyCenter<SceneEvent, Vector3, ItemDetails>.NotifyObservers(SceneEvent.AfterPlayerAnimation, mouseWorldPos, details);
        yield return new WaitForSeconds(0.25f);
        canInput = true;
        // isUsingTool = false;
    }
    private void OnDisable()
    {
        NotifyCenter<SceneEvent, Vector3, bool>.notifyCenter -= OnMovePlayer;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSceneChange;
        NotifyCenter<SceneEvent, Vector3, ItemDetails>.notifyCenter -= OnInputEvent;

    }
    private void OnMovePlayer(SceneEvent sceneEvent, Vector3 pos, bool arg3)
    {
        if (sceneEvent == SceneEvent.MovePlayer)
            transform.position = pos;

    }
    private void OnSceneChange(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.BeforeLoadScene)
        {
            canInput = false;

        }
        else if (sceneEvent == SceneEvent.AfterLoadScene)
        {
            canInput = true;

        }
    }

    private void Update()
    {
        if (canInput)
        {
            PlayerInput();
        }
        else
        {
            isMoving = false;
        }
        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (canInput)
            Movement();
    }
    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        //to prevent the Player from moving horizontally and vertically simultaneously
        // if (inputX == 0)
        // {
        //     inputY = Input.GetAxisRaw("Vertical");
        // }
        // if (inputY == 0)
        // {
        //     inputX = Input.GetAxisRaw("Horizontal");
        // }
        if (inputX != 0 && inputY != 0)
        {
            inputX = 0.6f * inputX;
            inputY = 0.6f * inputY;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }
        movementInput = new Vector2(inputX, inputY);
        isMoving = movementInput != Vector2.zero;
    }

    private void Movement()
    {
        rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (var anim in animators)
        {
            anim.SetBool("IsMoving", isMoving);
            anim.SetFloat("MouseX", mouseX);
            anim.SetFloat("MouseY", mouseY);
            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }
}
