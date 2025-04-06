using System.Collections;
using UnityEngine;

public class CoroutineRunner : Singleton<CoroutineRunner>
{
    public void Run(IEnumerator coroutine)
    {
        Instance.StartCoroutine(coroutine);
    }
}