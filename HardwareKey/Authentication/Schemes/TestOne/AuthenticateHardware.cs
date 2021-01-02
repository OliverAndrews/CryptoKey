using HardwareKey.Types;
using System;

namespace HardwareKey.Authentication.Schemes.TestOne
{
    public class AuthenticateHardware<T> : IAuthenticate<T>
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
            } else
            {
                Console.WriteLine("Failed to authenticate");
            }

        }

    }
}
