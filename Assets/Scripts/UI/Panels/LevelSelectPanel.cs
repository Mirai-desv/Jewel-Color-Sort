using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bảng chọn level mở từ Main Menu. Level đã thắng / đang chơi hiện bình thường và bấm được (chơi lại);
/// level chưa mở bị tối đi và không bấm được.
/// </summary>
public class LevelSelectPanel : BasePanel
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelSelectItem itemPrefab;
    [Tooltip("Content của ScrollView (nên gắn GridLayoutGroup).")]
    [SerializeField] private RectTransform content;

    private readonly List<LevelSelectItem> _items = new List<LevelSelectItem>();

    protected override void OnShow()
    {
        transform.SetAsLastSibling();
        Refresh();
    }

    private void Refresh()
    {
        if (levelManager == null || itemPrefab == null || content == null)
        {
            Debug.LogError("[LevelSelectPanel] Thiếu Level Manager / Item Prefab / Content.", this);
            return;
        }

        int count = levelManager.LevelCount;
        int highest = SaveManager.HighestUnlockedLevel;
        bool unlockAll = SaveManager.UnlockAllLevels;

        while (_items.Count < count)
        {
            LevelSelectItem item = Instantiate(itemPrefab, content);
            item.Clicked += OnItemClicked;
            _items.Add(item);
        }

        for (int i = 0; i < _items.Count; i++)
        {
            bool used = i < count;
            _items[i].gameObject.SetActive(used);
            if (!used) continue;

            LevelSelectItem.State state;
            if (unlockAll)
            {
                // Mở full level để playtest: level đã có điểm thì hiện Completed, còn lại đều mở (Current) để bấm được ngay
                bool completed = SaveManager.GetLevelBestScore(i) > 0;
                state = completed ? LevelSelectItem.State.Completed : LevelSelectItem.State.Current;
            }
            else
            {
                state =
                    i < highest ? LevelSelectItem.State.Completed :
                    i == highest ? LevelSelectItem.State.Current :
                    LevelSelectItem.State.Locked;
            }

            _items[i].Setup(i, state, SaveManager.GetLevelBestScore(i));
        }
    }

    private void OnItemClicked(int index)
    {
        if (GameManager.Instance != null) GameManager.Instance.PlayLevel(index);
    }
}