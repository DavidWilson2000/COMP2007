using UnityEngine;

public class BridgeSegment : MonoBehaviour
{
    public enum SegmentType { Straight, LeftTurn, RightTurn }
    public SegmentType segmentType;

    public void SetupBridgeType(string prefabName)
    {
        string lower = prefabName.ToLower();
        if (lower.Contains("l_turn"))
            segmentType = SegmentType.LeftTurn;
        else if (lower.Contains("r_turn"))
            segmentType = SegmentType.RightTurn;
        else
            segmentType = SegmentType.Straight;
    }
}
