using HardwareKey.Types;
using System.Management;

namespace HardwareKey.Authentication.Schemes.TestOne.Validator
{
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
}
