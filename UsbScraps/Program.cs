using System.Management;
using HardwareKey.Authentication.Schemes.TestOne;
using HardwareKey.Authentication.Schemes.TestOne.AuthFunction;
using HardwareKey.Authentication.Schemes.TestOne.Validator;

namespace UsbScraps
{
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = new AuthenticateHardware<ManagementBaseObject>(new ValidateDrive("42E7D729"), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }
}
