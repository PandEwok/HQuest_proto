using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    /// <summary>
    /// Appelle cette méthode depuis l'événement OnClick() de ton bouton UI.
    /// </summary>
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneChanger: Le nom de la scène n'a pas été renseigné dans l'Inspecteur.");
        }
    }

    /// <summary>
    /// Optionnel : charger une scène par son nom directement en paramètre.
    /// </summary>
    public void ChangeSceneByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            SceneManager.LoadScene(name);
        }
    }
}