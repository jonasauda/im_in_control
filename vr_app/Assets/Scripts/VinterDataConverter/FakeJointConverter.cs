using UnityEngine;
using System.Linq;

using VinteR.Model.Gen;
using System;
using static VinteR.Model.Gen.MocapFrame.Types;

/*
    The first body is the position of the foreign controller
    The second body is used to determine the connected object. If empty the joint should be destroyed
 */
class FakeJointConverter {

    public static Body convert(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, String hrri, String connectedHrri) {
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
            Points = {
                new VinteR.Model.Gen.MocapFrame.Types.Body.Types.Point {
                    Name = connectedHrri,
                    State = "",
                    Position = new Body.Types.Vector3 {
                        X = 0,
                        Y = 0,
                        Z = 0
                    }
                }
            },
        };
        return body;
    }

    public static MocapFrame createDestroyEvent(String hrri)
    {
        return new MocapFrame {
            Bodies = {
                new Body {
                    BodyType = MocapFrame.Types.Body.Types.EBodyType.RigidBody,
                    Name = hrri,
                    Centroid = new Body.Types.Vector3 {
                        X = 0,
                        Y = 0,
                        Z = 0
                    },
                    Rotation = new Body.Types.Quaternion {
                        X = 0.0f,
                        Y = 0.0f,
                        Z = 0.0f,
                        W = 0.0f
                    },
                    Points = {}
                }
            }
        };
    }

    public static bool isDeleteEvent(Body body)
    {
        return body.Points.Count <= 0;
    }

    public static FixedJointPlaceholder convert(Body body) {
        if (body.Points.Count >= 1) {
            var connectedPoint = body.Points[0];
            FixedJointPlaceholder fxPlaceholder = new FixedJointPlaceholder();
            fxPlaceholder.hrri = body.Name;
            fxPlaceholder.connecetdHrri = connectedPoint.Name;
            fxPlaceholder.hasFixedJoint = false;
            return fxPlaceholder;
        }
        return null;
    }

}