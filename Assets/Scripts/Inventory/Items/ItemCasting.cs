using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class ItemCasting : MonoBehaviour
    {
        private Transform originalItemTrans;
        private BoxCollider2D coll;
        [SerializeField] private float castSpeed = 4.5f;
        private bool isGrounded;
        private float distance;
        private Vector2 flyingDir;
        private Vector3 targetPos;
        private void Awake()
        {
            //get the original item's transform
            originalItemTrans = transform.GetChild(0);
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false; //it must be closed otherwise a collision event will be triggered.
        }

        private void Update()
        {
            CastItem();
            // Debug.Log(coll.enabled);
        }

        public void InitCastingItem(Vector3 target, Vector2 dir)
        {
            coll.enabled = false;
            flyingDir = dir;
            this.targetPos = target;
            distance = Vector3.Distance(target, transform.position);
            //the initial position when created
            // originalItemTrans.position = transform.position + Vector3.up * Settings.itemHeldHeight;
            originalItemTrans.position += Vector3.up * Settings.itemHeldHeight;
            // Debug.Log("originalItemTrans:" + originalItemTrans.position.y);
            // Debug.Log("transform:" + transform.position.y);
        }

        private void CastItem()
        {
            //Firstly, we need to understand that the initial position of the thrown item is actually the anchor point of the character, which is under the character's feet. When generating the object to be thrown, the original image flies out from the head position, while the shadow flies out from the starting position. The original image starts to descend until its Y-axis coordinate matches that of the parent object, indicating that it has landed on the ground. Then the throwing process is complete.
            isGrounded = originalItemTrans.position.y <= transform.position.y;

            float currentDistance = Vector3.Distance(transform.position, targetPos);

            if (currentDistance > 0.1f)
            {
                //this is the best code in this file
                transform.position += (Vector3)flyingDir * distance * castSpeed * Time.deltaTime;
            }

            if (!isGrounded)
            {
                //descending to the original height to the ground. 
                // Vector3.up = Vector3(0,1,0);
                originalItemTrans.position += Vector3.up * (-castSpeed) * Time.deltaTime;
            }
            else
            {
                originalItemTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}

/*
在写这段代码的时候，一是要注意扔到地图上图片的锚点应该是下方，第二就是图片和影子的层级关系，另外就是注意物品飞出去的起始坐标，它的position是世界的position
*/