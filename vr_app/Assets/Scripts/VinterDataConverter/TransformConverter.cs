using UnityEngine;
using Aristo;
using System.Linq;

using VinteR.Model.Gen;
using System;
using static VinteR.Model.Gen.MocapFrame.Types;
class TransformConverter {

    public static Body convert(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, String hrri) {
        Body body = new Body {
            BodyType = MocapFrame.Types.Body.Types.EBodyType.RigidBody,
            Name = hrri,
            Centroid = new Body.Types.Vector3 {
                X = position.x,
                Y = position.y,
                Z = position.z
            },
            Rotation = new VinteR.Model.Gen.MocapFrame.Types.Body.Types.Quaternion {
                X = rotation.x,
                Y = rotation.y,
                Z = rotation.z,
                W = rotation.w
            },
            Points = {}
        };
        return body;
    }

    public static GameObject convert(Body body) {
        GameObject go = new GameObject();
        go.transform.position = new UnityEngine.Vector3 {
            x = body.Centroid.X,
            y = body.Centroid.Y,
            z = body.Centroid.Z,
        };
        
        go.transform.rotation = new UnityEngine.Quaternion {
            x = body.Rotation.X,
            y = body.Rotation.Y,
            z = body.Rotation.Z,
            w = body.Rotation.W,
        };
        return go;
    }

}