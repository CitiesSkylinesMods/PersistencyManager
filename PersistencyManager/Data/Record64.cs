namespace PersistencyManager.Data {
    using MoveItIntegration;
    using System;
    using System.Collections.Generic;

    struct Record64 {
        internal InstanceID InstanceID;
        internal string Data64;

        static internal Record64? Create(InstanceID instanceID, MoveItIntegrationBase man) {
            object data = man.Copy(instanceID);
            if (data is null)
                return null;

            string data64 = man.Encode64(data);
            if (data64 == null)
                return null;

            return new Record64 { InstanceID = instanceID, Data64 = data64 };
        }

        internal object Paste(MoveItIntegrationBase man, Version version, Dictionary<InstanceID, InstanceID> map) {
            object data = man.Decode64(Data64, version);
            man.Paste(InstanceID, data, map);
            return data;
        }
    }
}
