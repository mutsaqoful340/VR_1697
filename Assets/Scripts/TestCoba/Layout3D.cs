using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Layout3D : MonoBehaviour
{
    public enum LayoutType { Grid, LineX, LineY, LineZ }
    public LayoutType layoutType = LayoutType.Grid;

    public enum Alignment
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        LowerLeft,
        LowerCenter,
        LowerRight
    }
    public Alignment alignment = Alignment.MiddleCenter;

    [Min(1)]
    public int columns = 5;
    public float spacing = 2f;

    [Header("Transitions")]
    public bool animateTransitions = true; // runtime only
    public float transitionDuration = 0.25f;
    public bool stagger = false;
    public float staggerDelay = 0.03f;

    // runtime coroutine tracking so we can cancel/restart per-child
    private Dictionary<Transform, Coroutine> activeCoroutines = new Dictionary<Transform, Coroutine>();

    void OnValidate()
    {
        if (columns < 1) columns = 1;
        Arrange();
    }

    private void Start()
    {
        // Ensure layout is correct at runtime start
        Arrange();
    }

    // Called when children are added/removed in runtime or edit mode
    private void OnTransformChildrenChanged()
    {
        Arrange();
    }

    public void Arrange()
    {
        int count = transform.childCount;

        if (layoutType == LayoutType.Grid)
        {
            int rows = Mathf.CeilToInt(count / (float)columns);

            float totalWidth = (columns - 1) * spacing;
            float totalHeight = (rows - 1) * spacing;

            // alignment offsets (centered by default)
            float offsetX = 0;
            float offsetZ = 0;

            switch (alignment)
            {
                case Alignment.UpperLeft:
                    offsetX = 0;
                    offsetZ = 0;
                    break;

                case Alignment.UpperCenter:
                    offsetX = -totalWidth / 2;
                    offsetZ = 0;
                    break;

                case Alignment.UpperRight:
                    offsetX = -totalWidth;
                    offsetZ = 0;
                    break;

                case Alignment.MiddleLeft:
                    offsetX = 0;
                    offsetZ = -totalHeight / 2;
                    break;

                case Alignment.MiddleCenter:
                    offsetX = -totalWidth / 2;
                    offsetZ = -totalHeight / 2;
                    break;

                case Alignment.MiddleRight:
                    offsetX = -totalWidth;
                    offsetZ = -totalHeight / 2;
                    break;

                case Alignment.LowerLeft:
                    offsetX = 0;
                    offsetZ = -totalHeight;
                    break;

                case Alignment.LowerCenter:
                    offsetX = -totalWidth / 2;
                    offsetZ = -totalHeight;
                    break;

                case Alignment.LowerRight:
                    offsetX = -totalWidth;
                    offsetZ = -totalHeight;
                    break;
            }

            // Apply layout
            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                int row = i / columns;
                int col = i % columns;

                float x = col * spacing + offsetX;
                float z = row * spacing + offsetZ;

                Vector3 targetLocalPos = new Vector3(x, 0, z);
                Quaternion targetLocalRot = Quaternion.identity;

                if (animateTransitions && Application.isPlaying)
                {
                    float delay = stagger ? i * staggerDelay : 0f;
                    StartAnimatedMove(child, targetLocalPos, targetLocalRot, transitionDuration, delay);
                }
                else
                {
                    // instant placement (edit mode or transitions disabled)
                    StopActiveCoroutineIfAny(child);
                    child.localPosition = targetLocalPos;
                    child.localRotation = targetLocalRot;
                }
            }
        }
        else
        {
            // Simple line layouts — align only center
            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                Vector3 pos = Vector3.zero;

                if (layoutType == LayoutType.LineX)
                    pos = new Vector3(i * spacing, 0, 0);

                else if (layoutType == LayoutType.LineY)
                    pos = new Vector3(0, i * spacing, 0);

                else if (layoutType == LayoutType.LineZ)
                    pos = new Vector3(0, 0, i * spacing);

                if (animateTransitions && Application.isPlaying)
                {
                    float delay = stagger ? i * staggerDelay : 0f;
                    StartAnimatedMove(child, pos, Quaternion.identity, transitionDuration, delay);
                }
                else
                {
                    StopActiveCoroutineIfAny(child);
                    child.localPosition = pos;
                }
            }
        }
    }

    private void StartAnimatedMove(Transform child, Vector3 targetLocalPos, Quaternion targetLocalRot, float duration, float delay)
    {
        // cancel any existing animation for this child
        StopActiveCoroutineIfAny(child);

        // store the key so we don't reference a destroyed transform later
        Transform key = child;
        Coroutine c = StartCoroutine(AnimatedMoveCoroutine(key, targetLocalPos, targetLocalRot, duration, delay));
        try
        {
            activeCoroutines[key] = c;
        }
        catch (System.Exception)
        {
            // if the key is invalid/destroyed, just don't store it
        }
    }

    private void StopActiveCoroutineIfAny(Transform child)
    {
        if (activeCoroutines.TryGetValue(child, out Coroutine existing))
        {
            if (existing != null) StopCoroutine(existing);
            activeCoroutines.Remove(child);
        }
    }

    private IEnumerator AnimatedMoveCoroutine(Transform t, Vector3 targetLocalPos, Quaternion targetLocalRot, float duration, float delay)
    {
        // local key reference for dictionary cleanup
        Transform key = t;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
            // if the transform was destroyed while waiting, exit
            if (t == null) yield break;
        }

        // If the transform was destroyed, abort animation
        if (t == null) yield break;

        Vector3 startPos = t.localPosition;
        Quaternion startRot = t.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // if the transform was destroyed mid-animation, stop
            if (t == null) break;

            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            // ease-out cubic
            u = 1f - Mathf.Pow(1f - u, 3f);
            t.localPosition = Vector3.Lerp(startPos, targetLocalPos, u);
            t.localRotation = Quaternion.Slerp(startRot, targetLocalRot, u);
            yield return null;
        }

        if (t != null)
        {
            t.localPosition = targetLocalPos;
            t.localRotation = targetLocalRot;
        }

        // Remove dictionary entry using the original key reference
        if (key != null)
        {
            activeCoroutines.Remove(key);
        }
    }

    private void OnDisable()
    {
        // stop any running coroutines when disabled
        foreach (var kv in activeCoroutines)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        activeCoroutines.Clear();
    }
}
