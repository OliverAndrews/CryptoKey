
using System.Collections.Generic;

namespace HardwareKey.Authentication.Schemes.TestOne.Types
{
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel(string name);

        bool HasDevicesWithLabel(string name);
    }

}
