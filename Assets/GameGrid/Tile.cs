using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    private MeshRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    public void Init(bool isOffset)
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        _renderer.material.color = isOffset ? _offsetColor : _baseColor;
    }

    void OnMouseEnter()
    {
        Debug.Log("kkk");
        //_highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        //_highlight.SetActive(false);
    }
}