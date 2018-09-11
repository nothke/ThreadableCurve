/// Threadable AnimationCurve
/// By Nothke
/// 
/// The in and out wrap modes are always 'clamp',
/// no other wrap modes are supported for now
/// 

using UnityEngine;

[System.Serializable]
public class ThreadableCurve
{
    [SerializeField]
    private AnimationCurve curve;

    Keyframe[] keyframes;

    /// <summary>
    /// Call this outside of the thread to cache keyframes from the Unity AnimationCurve
    /// </summary>
    public void CacheKeyframes()
    {
        int l = curve.keys.Length;
        keyframes = new Keyframe[l];
        System.Array.Copy(curve.keys, keyframes, l);
    }

    /// <summary>
    /// Evaluate the curve at time. 
    /// Make sure to call CacheKeyframes() at least once before outside of the thread.
    /// Only 'clamp' wrap mode is supported.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public float Evaluate(float t)
    {
        if (keyframes == null || keyframes.Length == 0) return 0;
        if (keyframes.Length == 1) return keyframes[0].value;

        if (t < keyframes[0].time)
            return keyframes[0].value;

        for (int i = 1; i < keyframes.Length; i++)
        {
            if (t < keyframes[i].time)
            {
                float kt = Mathf.InverseLerp(keyframes[i - 1].time, keyframes[i].time, t);
                return Evaluate(kt, keyframes[i - 1], keyframes[i]);
            }
        }

        return keyframes[keyframes.Length - 1].value;
    }

    // From: http://answers.unity.com/answers/508835/view.html
    float Evaluate(float t, Keyframe keyframe0, Keyframe keyframe1)
    {
        float dt = keyframe1.time - keyframe0.time;

        float m0 = keyframe0.outTangent * dt;
        float m1 = keyframe1.inTangent * dt;

        float t2 = t * t;
        float t3 = t2 * t;

        float a = 2 * t3 - 3 * t2 + 1;
        float b = t3 - 2 * t2 + t;
        float c = t3 - t2;
        float d = -2 * t3 + 3 * t2;

        return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
    }
}
