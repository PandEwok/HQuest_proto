using UnityEngine;

public class TrapBehaviour : MonoBehaviour
{
    [Header("Configuration du Piège")]
    [SerializeField] private TrapType trapType; // <-- Ceci va apparaître dans l'Inspecteur

    private ITrap trapLogic;
    public ITrap TrapLogic => trapLogic;
    public TrapType CurrentTrapType => trapType;

    private void Awake()
    {
        InitializeTrap();
    }

    public void InitializeTrap()
    {
        if (trapLogic != null) return;

        CombatDiceManager diceManager = FindFirstObjectByType<CombatDiceManager>();

        switch (trapType)
        {
            case TrapType.PitTrap:
                trapLogic = new PitTrap();
                break;
            case TrapType.SpearTrap:
                trapLogic = new SpearTrap(diceManager);
                break;
            case TrapType.FallingBlockTrap:
                trapLogic = new FallingBlockTrap(diceManager);
                break;
        }
    }

    public void OnTriggerSpring(Character hero)
    {
        if (trapLogic != null && !trapLogic.IsSprung)
        {
            trapLogic.OnSpring(hero);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Contact avec " + other.name);
        Hero hero = other.GetComponentInParent<Hero>();
        if (hero == null) return;

        OnTriggerSpring(hero);
    }

    public void OnSearched(Character hero)
    {
        if (trapLogic != null && !trapLogic.IsDiscovered)
        {
            trapLogic.OnDiscovered(hero);
        }
    }
}