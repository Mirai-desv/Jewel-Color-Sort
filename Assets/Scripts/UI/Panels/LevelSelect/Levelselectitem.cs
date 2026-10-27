using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>1 ô level trong bảng chọn level. Prefab cần có Button (đang active).</summary>
[RequireComponent(typeof(Button))]
public class LevelSelectItem : MonoBehaviour
{
    public enum State { Locked, Current, Completed }

    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text scoreText;        // tuỳ chọn: điểm cao nhất
    [Tooltip("Lớp phủ tối (Image đen, alpha ~0.45) bật khi level chưa mở.")]
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject completedMarker; // tuỳ chọn: dấu tick

    public event Action<int> Clicked;

    private Button _button;
    private int _index;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            var nav = _button.navigation;
            nav.mode = Navigation.Mode.None;
            _button.navigation = nav;
        }
        _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(HandleClick);
    }

    public void Setup(int index, State state, int bestScore)
    {
        _index = index;

        if (numberText != null) numberText.SetText("{0}", index + 1);

        bool locked = state == State.Locked;
        if (_button != null) _button.interactable = !locked;
        if (lockedOverlay != null) lockedOverlay.SetActive(locked);
        if (completedMarker != null) completedMarker.SetActive(state == State.Completed);

        if (scoreText != null)
        {
            bool show = state == State.Completed && bestScore > 0;
            scoreText.gameObject.SetActive(show);
            if (show) scoreText.SetText("{0}", bestScore);
        }
    }

    private void HandleClick()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        AudioManager.PlaySfx(Sfx.Click);
        Clicked?.Invoke(_index);
    }
}