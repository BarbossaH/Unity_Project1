using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    //for multiple types of particle prefabs, such as trees', rock's, and others
    public List<GameObject> poolPrefabs;

    //each effect has their own object pool
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();

    private void Start()
    {
        CreatePool();
    }
    private void CreatePool()
    {
        foreach (GameObject item in poolPrefabs)
        {
            Transform parent = new GameObject(item.name).transform;
            parent.SetParent(transform);

            ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
                () => Instantiate(item, parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }, true, 10, 20
            );
            poolEffectList.Add(newPool);
        }
    }
    private void OnEnable()
    {
        NotifyCenter<SceneEvent, ParticleEffectType, Vector3>.notifyCenter += OnPlayEffect;
    }

    private void OnDisable()
    {
        NotifyCenter<SceneEvent, ParticleEffectType, Vector3>.notifyCenter -= OnPlayEffect;

    }

    private void OnPlayEffect(SceneEvent sceneEvent, ParticleEffectType type, Vector3 pos)
    {
        if (sceneEvent == SceneEvent.PlayParticleEffect)
        {
            var objPool = type switch
            {
                ParticleEffectType.LeaveFalling_01 => poolEffectList[0],
                ParticleEffectType.LeaveFalling_02 => poolEffectList[1],
                _ => null
            };

            GameObject obj = objPool.Get();
            obj.transform.position = pos;
            StartCoroutine(ReleaseRoutine(objPool, obj));
        }
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool, GameObject gameObject)
    {
        yield return new WaitForSeconds(2f);
        pool.Release(gameObject);
    }
}
