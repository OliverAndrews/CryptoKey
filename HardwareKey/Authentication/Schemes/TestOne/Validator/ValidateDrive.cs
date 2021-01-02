using HardwareKey.Types;
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace HardwareKey.Authentication.Schemes.TestOne.Validator
{
    public class ValidateDrive : IValidate<ManagementBaseObject>
    {

        private string _expected;
        public ValidateDrive(string expected)
        {
            _expected = CreateHash(expected);
        }
        private string CreateHash(string target)
        {
            string stringHash;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(target));
                stringHash = BitConverter.ToString(hash).Replace("-", "");
            }
            return stringHash;
        }

        private bool RunCheckOnDrive(ManagementBaseObject drive)
        {
            var checkHash = CreateHash(drive["VolumeSerialNumber"].ToString());
            return (checkHash == _expected);
        }

        public bool Check(ManagementBaseObject drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
}
