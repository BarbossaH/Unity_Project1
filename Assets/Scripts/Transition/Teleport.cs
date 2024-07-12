using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Transition
{

    public class Teleport : MonoBehaviour
    {
        public string sceneToGo;
        public Vector3 posToGo;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                NotifyCenter<SceneEvent, string, Vector3>.NotifyObservers(SceneEvent.Transition, sceneToGo, posToGo);
            }
        }
    }
}