using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace UsbScraps
{
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = new AuthenticateHardware<ManagementBaseObject>(new ValidateDrive(), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }

    #region Data Objects

    class DriveQuery
    {
        public string Kind { get; set; }
        public string Database { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Task Definition
    class AuthTest : IAuthTask
    {
        public void Main()
        {
            Console.WriteLine("Done");
        }
    }
    #endregion

    #region Validator Definition
    class ValidateDrive : IValidate<ManagementBaseObject>
    {
        private bool RunCheckOnDrive(ManagementBaseObject drive)
        {
            return (drive["VolumeName"].ToString() == "TESTDISK");
        }

        public bool Check(ManagementBaseObject drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
    #endregion

    #region Interfaces
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel(string name);

        bool HasDevicesWithLabel(string name);
    }

    interface IValidate<T>
    {
        bool Check(T certificate);
    }

    interface IAuthTask
    {
        void Main();
    }
    #endregion

    #region Implementations
    class AuthenticateHardware<T>
    {
        private IValidate<T> _validator;
        private T _certificate;

        public AuthenticateHardware(IValidate<T> validator, T certificate)
        {
            _certificate = certificate;
            _validator = validator;
        }

        public void AuthenticateThen(IAuthTask next)
        {
            if (_validator.Check(_certificate))
            {
                next.Main();
            }

        }

    }

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
    #endregion
}
