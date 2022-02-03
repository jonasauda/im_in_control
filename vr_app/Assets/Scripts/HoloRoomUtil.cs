using UnityEngine;

class HoloRoomUtil {
    
    public static Vector3 convertFromUnityToHoloRoom(Vector3 unityVector) {
        return Vector3.Scale(unityVector, new Vector3(-1, 1, 1)) * 1000f;
    }

    public static Vector3 convertFromHoloRoomToUnity(Vector3 holoRoomVector) {
        return Vector3.Scale(holoRoomVector, new Vector3(-1, 1, 1)) / 1000f;
    }

    public static Quaternion convertRotation(Quaternion rotation) {
        return new Quaternion(rotation.x, (-1) * rotation.y, (-1) * rotation.z, rotation.w);
    }

}