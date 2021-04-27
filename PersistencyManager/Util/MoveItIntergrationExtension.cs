namespace PersistencyManager.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MoveItIntegration;
    using API;

    internal static class MoveItIntergrationExtension {
        internal static object CopyNode(this MoveItIntegrationBase man, ushort nodeID) =>
            man.Copy(new InstanceID { NetNode = nodeID });
        internal static object CopySegment(this MoveItIntegrationBase man, ushort segmentID) =>
            man.Copy(new InstanceID { NetSegment = segmentID });
        internal static object CopySegmentEnd(this MoveItIntegrationBase man, ushort segmentID, bool start) =>
            man.Copy(InstanceIDExtension.SegmentEnd(segmentID, start));
        
    }
}
