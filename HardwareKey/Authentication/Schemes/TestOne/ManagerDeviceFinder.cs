using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using HardwareKey.Authentication.Schemes.TestOne.Types;

namespace HardwareKey.Authentication.Schemes
{

    class ManagerDeviceFinder : IDriveFinder<ManagementBaseObject>
    {

        public ManagementBaseObject GetDeviceByLabel(string name)
        {
            var search = new ManagementObjectSearcher($"SELECT * FROM Win32_LogicalDisk");
            var results = search.Get();
            return results.OfType<ManagementBaseObject>()
                .Where(x => x["VolumeName"].ToString() == name)
                .FirstOrDefault();
        }

        public IEnumerable<ManagementBaseObject> GetDevicesWithLabel(string name)
        {
            throw new NotImplementedException();
        }

        public bool HasDeviceOfName(string name)
        {
            throw new NotImplementedException();
        }

        public bool HasDevicesWithLabel(string name)
        {
            throw new NotImplementedException();
        }
    }
}
