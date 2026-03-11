using UnityEngine;

public static class VectorExtensions {

    public static Vector3 Absolute(this Vector3 vector) {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static Vector3 Project(this Vector3 vector, Vector3 planeNormal) {
        float mag = vector.magnitude;
        vector = Vector3.ProjectOnPlane(vector, planeNormal).normalized;
        return vector * mag;
    }


    public static Vector3 NormalizeWithoutY(this Vector3 vector) {
        return new Vector3(vector.x, 0, vector.z).normalized;
    }


    public static Vector3 Add(this Vector3 vector, float num) {
        return new Vector3(vector.x + num, vector.y + num, vector.z + num);
    }


    public static float DeltaAngle(this Vector3 vector, Vector3 other) {
        float xDiff = Mathf.DeltaAngle(vector.x, other.x);
        float yDiff = Mathf.DeltaAngle(vector.y, other.y);
        float zDiff = Mathf.DeltaAngle(vector.z, other.z);
        return Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
    }

    public static float DotXZ(this Vector3 vector, Vector3 other) {
        return Vector2.Dot(new Vector2(vector.x, vector.z), new Vector2(other.x, other.z));
    }

    /// <summary>
    /// a wonderful little friend for when Vector3.Lerp starts freakin the fuck out
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="other"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    public static Vector3 LerpAngleTo(this Vector3 vector, Vector3 other, float delta) {
        return new Vector3(
            Mathf.LerpAngle(vector.x, other.x, delta),
            Mathf.LerpAngle(vector.y, other.y, delta),
            Mathf.LerpAngle(vector.z, other.z, delta)
        );
    }

    public static float XZMagnitude(this Vector3 vector) {
        return new Vector2(vector.x, vector.z).magnitude;
    }


    public static Vector3 ApproachYZero(this Vector3 vector, float delta) {
        return Vector3.Lerp(vector, new Vector3(vector.x, 0, vector.z), delta);
    }

    public static float YawAngle(this Vector3 vector, Vector3 other) {
        vector.y = 0; other.y = 0;
        float angle = Vector3.Angle(vector.normalized, other.normalized);
        Vector3 cross = Vector3.Cross(vector, other);
        return cross.y < 0 ? -angle : angle;
    }
}
