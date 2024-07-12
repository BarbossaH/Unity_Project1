using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFarm.Transition
{
    public class TransitionManager : MonoBehaviour
    {
        public string defaultSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;
        private bool isFading;

        //string is for scene name
        private void OnEnable()
        {
            NotifyCenter<SceneEvent, string, Vector3>.notifyCenter += OnTransitionEvent;
        }

        private void OnDisable()
        {
            NotifyCenter<SceneEvent, string, Vector3>.notifyCenter -= OnTransitionEvent;
        }


        private IEnumerator Start()
        {
            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
            yield return LoadSceneSetActive(defaultSceneName);
            // StartCoroutine(LoadSceneSetActive(defaultSceneName));
            // fadeCanvasGroup = GameObject.FindGameObjectWithTag("FadeCanvas").GetComponent<CanvasGroup>();
            // Debug.Log(fadeCanvasGroup.name);
            NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.AfterLoadScene, true, true);
        }

        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newScene);
        }

        private IEnumerator Transition(string sceneName, Vector3 pos)
        {
            yield return Fade(1);
            NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.BeforeLoadScene, true, true);

            //unload the current scene
            // yield return SceneManager.UnloadSceneAsync(defaultSceneName);
            // yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneSetActive(sceneName);
            NotifyCenter<SceneEvent, Vector3, bool>.NotifyObservers(SceneEvent.MovePlayer, pos, true);
            NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.AfterLoadScene, true, true);
            yield return Fade(0);

        }

        private void OnTransitionEvent(SceneEvent sceneEvent, string scene, Vector3 pos)
        {
            if (sceneEvent == SceneEvent.Transition)
            {
                // Debug.Log(scene);
                if (!isFading)
                    StartCoroutine(Transition(scene, pos));
            }

        }
        private IEnumerator Fade(float targetAlpha)
        {
            isFading = true;
            fadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.loadingFadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            fadeCanvasGroup.blocksRaycasts = false;
            isFading = false;
        }

        //about items in the scenes


    }
}