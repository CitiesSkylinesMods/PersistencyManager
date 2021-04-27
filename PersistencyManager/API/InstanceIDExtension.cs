namespace PersistencyManager.API {
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons;

    /// <summary>
    ///  super-impose  segment end upon InstanceID.
    /// </summary>
    internal static class InstanceIDExtension { 
        private const uint SEGMENT_END_BIT = 0x10000u; // MAX_SEGMENT_COUNT is 0x9000

        internal const  InstanceType InstanceType_SegmentEnd = (InstanceType)0x15;

        internal static InstanceID SetSegmentEnd(ref this InstanceID instanceID, ushort segmentID, bool start) {
            instanceID.Type = InstanceType_SegmentEnd;
            if (start)
                instanceID.Index = segmentID;
            else
                instanceID.Index = segmentID | SEGMENT_END_BIT;
            return instanceID;
        }

        internal static ushort GetSegmendEnd(ref this InstanceID instanceID, out bool start) {
            if(instanceID.Type != InstanceType_SegmentEnd) {
                start = default;
                return 0;
            }

            uint index = instanceID.Index;
            start = index < SEGMENT_END_BIT;
            return (ushort)index; // SEGMENT_END_BIT does not fit ushort
        }

        internal static InstanceID SegmentEnd(ushort segmentID, bool start) {
            var ret = new InstanceID();
            return ret.SetSegmentEnd(segmentID, start);
        }

        internal static ushort GetSourceNodeID(this Dictionary<InstanceID, InstanceID> map,
            InstanceID targetInstanceID) {
            if (targetInstanceID.Type == InstanceType.NetNode) {
                return map.GetSourceInstanceID(targetInstanceID).NetNode;
            } else if(targetInstanceID.Type == InstanceType_SegmentEnd) {
                ushort segmentID = targetInstanceID.GetSegmendEnd(out bool startNode);
                ushort nodeID = segmentID.ToSegment().GetNode(startNode);
                return map.GetSourceNodeID(nodeID);
            }
            return 0;
        }

        internal static ushort GetSourceNodeID(this Dictionary<InstanceID, InstanceID> map, ushort targetNodeID) {
            InstanceID targetInstanceID = new InstanceID { NetNode = targetNodeID };
            return map.GetSourceInstanceID(targetInstanceID).NetNode;
        }

        internal static InstanceID GetSourceInstanceID(
            this Dictionary<InstanceID, InstanceID> map,
            InstanceID targetInstanceID) {
            return map.FirstOrDefault(pair => pair.Value == targetInstanceID).Key;
        }

    }
}
