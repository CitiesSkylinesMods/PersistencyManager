namespace PersistencyManager.Data {
    using MoveItIntegration;
    using PersistencyManager.Util;
    using KianCommons;

    class SegmentRecord {
        internal ushort SegmentID;
        internal ushort StartNodeID;
        internal ushort EndNodeID;
        internal object Segment;
        internal object SegmenStart;
        internal object SegmenEnd;

        static internal SegmentRecord? Create(ushort segmentID, MoveItIntegrationBase man) {
            object segment = man.CopySegment(segmentID);
            object segmentStart = man.CopySegmentEnd(segmentID, true);
            object segmentEnd = man.CopySegmentEnd(segmentID, false);

            if (segment is null && segmentStart is null && segmentEnd is null)
                return null;

            return new SegmentRecord {
                SegmentID = segmentID,
                StartNodeID = segmentID.ToSegment().m_startNode,
                EndNodeID = segmentID.ToSegment().m_endNode,
                Segment = segment,
                SegmenStart = segmentStart,
                SegmenEnd = segmentEnd,
            };
        }
    }
}
