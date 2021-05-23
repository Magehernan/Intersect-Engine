using Intersect.Client.UI.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [SerializeField]
    private FillBar progressBar = default;

    AsyncOperation operation = null;
    private void Start()
    {
        operation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }

    private void Update()
    {
        progressBar.ChangeValue(operation.progress);
    }
}
