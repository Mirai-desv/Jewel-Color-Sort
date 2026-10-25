using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UISequence : MonoBehaviour
{
    public enum Kind
    {
        Pop,
        Fade,
        SlideUp
    }

    [System.Serializable]
    public class Step
    {
        public RectTransform target;
        public Kind kind = Kind.Pop;
        [Tooltip("Chờ bao nhiêu giây sau khi bước trước BẮT ĐẦU.")]
        [Min(0f)] public float delay = 0.15f;
        [Min(0.01f)] public float duration = 0.35f;
    }

    [SerializeField] private List<Step> steps = new List<Step>();
    [SerializeField] private float slideDistance = 80f;

    private struct Origin
    {
        public Vector3 scale;
        public Vector2 pos;
        public CanvasGroup group;
    }

    private Origin[] _origins;
    private bool _cached;

    private void Cache()
    {
        if(_cached) return;
        _origins = new Origin[steps.Count];
        for(int i = 0; i < steps.Count; i++)
        {
            RectTransform t = steps[i].target;
            if(t == null) continue;
            
            CanvasGroup g = t.GetComponent<CanvasGroup>();
            if(g == null) g = t.gameObject.AddComponent<CanvasGroup>();
            _origins[i] = new Origin {scale = t.localScale, pos = t.anchoredPosition, group = g};
        }
        _cached = true;
    }

    public void Play()
    {
        Cache();
        StopAllCoroutines();
        // Đưa mọi phần tử về trạng thái "chưa hiện"
        for(int i = 0; i < steps.Count; i++)
        {
            Step s = steps[i];
            if(s.target == null) continue;
            Origin o = _origins[i];
            o.group.alpha = s.kind == Kind.Pop ? 1f : 0f;
            o.group.interactable = false;
            o.group.blocksRaycasts = false;
            s.target.localScale = s.kind == Kind.Pop ? Vector3.zero : o.scale;
            s.target.anchoredPosition = s.kind == Kind.SlideUp ? o.pos + Vector2.down * slideDistance : o.pos;
        }
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        for(int i = 0; i < steps.Count; i++)
        {
            if(steps[i].delay > 0f) yield return new WaitForSecondsRealtime(steps[i].delay);
            if(steps[i].target != null) StartCoroutine(Animate(i));
        }
    }

    private IEnumerator Animate(int i)
    {
        Step s = steps[i];
        Origin o = _origins[i];

        for(float t = 0f; t < s.duration; t += Time.unscaledDeltaTime)
        {
            Apply(s, o, t / s.duration);
            yield return null;
        }

        Apply(s, o, 1f);
        o.group.interactable = true;
        o.group.blocksRaycasts = true;
    }

    private void Apply(Step s, Origin o, float k)
    {
        switch(s.kind)
        {
            case Kind.Pop:
                s.target.localScale = o.scale * EaseOutBack(k);
                break;
            case Kind.Fade:
                o.group.alpha = k;
                break;
            case Kind.SlideUp:
                float e = 1f - Mathf.Pow(1f - k, 3f);
                o.group.alpha = e;
                s.target.anchoredPosition = Vector2.LerpUnclamped(o.pos + Vector2.down * slideDistance, o.pos, e);
                break;
        }
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }
}