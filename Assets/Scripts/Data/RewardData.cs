using UnityEngine;

// Vật phẩm nhận được sau khi qua màn (hiện ở RewardPanel)
[CreateAssetMenu(fileName = "RewardData", menuName = "Scriptable Objects/RewardData")]
public class RewardData : ScriptableObject
{
    [Tooltip("ID duy nhất để lưu vào kho đồ (không chứa dấu phẩy). Để trống thì dùng tên asset.")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [TextArea(1, 3)]
    [SerializeField] private string description;
    [Tooltip("Số xu thưởng thêm (ngoài Win Reward trong GameConfig).")]
    [SerializeField, Min(0)] private int bonusCoins = 0;

    public string ItemId => string.IsNullOrEmpty(itemId) ? name : itemId;
    public string ItemName => string.IsNullOrEmpty(itemName) ? name : itemName;
    public Sprite Icon => icon;
    public string Description => description;
    public int BonusCoins => bonusCoins;
}