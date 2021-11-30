using System;
using System.Linq;
using System.Collections.Generic;
using VinteR.Model.Gen;
using UnityEngine;
using static VinteR.Model.Gen.MocapFrame.Types;

public class GameEvent
{
    public enum EventType { BlankInteraction, StartStudy, StopStudy }

    private static System.Random rnd = new System.Random();

    public EventType Type;
    public string ID;
    public List<string> data;

    public GameEvent(EventType type, List<string> data)
    {
        this.Type = type;
        this.ID = GenerateId();
        this.data = data;
    }

    public GameEvent(EventType type, string eventId, List<string> data)
    {
        this.Type = type;
        this.ID = eventId;
        this.data = data;
    }

    public static Body Serialize(GameEvent gameEvent)
    {
        Body result = new Body
        {
            Name = "",
            Points = { },
            BodyType = Body.Types.EBodyType.RigidBody,
            Centroid = new Body.Types.Vector3 { X = 0, Y = 0, Z = 0 },
            Rotation = new Body.Types.Quaternion { X = 0, Y = 0, Z = 0, W = 0 },
        };

        result.Points.Add(CreatePoint(gameEvent.Type.ToString()));
        result.Points.Add(CreatePoint(gameEvent.ID));
        gameEvent.data.ForEach(data =>
            result.Points.Add(CreatePoint(data)
        ));

        return result;
    }

    private static Body.Types.Point CreatePoint(string name)
    {
        return new Body.Types.Point
        {
            Name = name,
            Position = new Body.Types.Vector3 { X = 0, Y = 0, Z = 0 },
        };
    }

    public static GameEvent Parse(Body eventBody)
    {
        List<string> data = eventBody.Points.Select(p => p.Name).ToList();
        string eventName = data[0];
        string eventId = data[1];
        Enum.TryParse(eventName, out EventType eventType);
        var eventData = data.GetRange(2, data.Count - 2);
        return new GameEvent(eventType, eventId, eventData);
    }

    private static string GenerateId()
    {
        return rnd.Next().ToString();
    }
}
