using Aristo;
using System.Linq;

using VinteR.Model.Gen;
using static VinteR.Model.Gen.MocapFrame.Types;
using static VinteR.Model.Gen.MocapFrame.Types.Body.Types;
using static Config;
class HandConverter {

    public static Body convert(GestureResult hand, bool isLeft, HrriLocation hrriLocation, HrriGroup hrriGroup) {
        var points = hand.points.Select((p, i) => {
            return new Point {
                Name = i + "",
                Position = new VinteR.Model.Gen.MocapFrame.Types.Body.Types.Vector3() {
                    X = p.x,
                    Y = p.y,
                    Z = p.z
                }
            };
        }).ToArray();

        Body body = new Body {
            BodyType = MocapFrame.Types.Body.Types.EBodyType.Hand,
            Name = HrriUtil.CreateHrri(hrriLocation, hrriGroup, HrriObject.HAND, isLeft ? "LEFT" : "RIGHT"),
            Centroid = new Body.Types.Vector3 {
                X = hand.position.x,
                Y = hand.position.y,
                Z = hand.position.z
            },
            Rotation = new VinteR.Model.Gen.MocapFrame.Types.Body.Types.Quaternion {
                X = hand.rotation.x,
                Y = hand.rotation.y,
                Z = hand.rotation.z,
                W = hand.rotation.w
            },
            Points = {}
        };
        body.Points.Add(points);
        return body;
    }

    public static GestureResult convert(Body body) {
        GestureResultRaw hand = new GestureResultRaw();
        hand.points = body.Points.Select(p => new UnityEngine.Vector3(p.Position.X, p.Position.Y, p.Position.Z)).ToArray();
        hand.gesture = GestureType.Unknown;
        hand.isLeft = body.Name.Contains("LEFT");
        return new GestureResult(hand);
    }

}