using UnityEngine;

public enum RewardType
{
    Coin
}

[CreateAssetMenu(
    fileName = "RewardData",
    menuName = "Scriptable Objects/RewardData"
)]
public class RewardData : ScriptableObject
{
    [SerializeField] private RewardType type = RewardType.Coin;

    [SerializeField, Min(0)]
    private int amount = 10;

    [SerializeField] private Sprite icon;

    public RewardType Type => type;
    public int Amount => amount;
    public Sprite Icon => icon;
}