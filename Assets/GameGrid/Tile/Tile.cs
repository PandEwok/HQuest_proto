using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private MeshRenderer[] _renderers;
    [SerializeField] public GameObject _highlight;
    [SerializeField] public GameObject actionIndicator;

    [SerializeField] public GameObject baseMesh;

    [HideInInspector] public List<GameObject> walls;

    public GameObject surface;

    public void Init(bool isOffset)
    {
        //_renderer = GetComponentInChildren<MeshRenderer>();
        Color newColor = isOffset ? _offsetColor : _baseColor;

        ChangeColor(newColor);
    }

    public void ChangeColor(Color newColor)
    {
        foreach (var _renderer in _renderers)
        {
            Material newMat = new Material(_renderer.material);
            newMat.SetColor("_BaseColor", newColor);
            _renderer.material = newMat;
        }
    }

    public void ChangeIndicator(Color newColor)
    {
        Material newMat = new Material(actionIndicator.GetComponent<MeshRenderer>().material);
        newMat.SetColor("_BaseColor", newColor);
        actionIndicator.GetComponent<MeshRenderer>().material = newMat;
    }

    void OnMouseEnter() {
        //Debug.Log("kkk");
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
}