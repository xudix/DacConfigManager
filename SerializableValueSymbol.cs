using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT;
using TwinCAT.TypeSystem;
using TwinCAT.Ads.TypeSystem;

namespace DacConfigManager
{
    [Serializable]
    public class SerializableValueSymbol
    {

        
        public string Path;
        public string Name;
        public dynamic Value;

        public SerializableValueSymbol(DynamicSymbol symbol)
        {
            Path = symbol.InstancePath;
            Name = symbol.InstanceName;
            Value = symbol.ReadValue();
        }

        public SerializableValueSymbol()
        {
            Path = "";
            Name = "";
            Value = false;
        }
    }
}
