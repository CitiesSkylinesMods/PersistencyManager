namespace PersistencyManager.API {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal interface IReleaser {
        void Release(InstanceID instanceID);
    }
}
