namespace HardwareKey.Types
{
    interface IAuthenticate<T>
    {
        void AuthenticateThen(IAuthTask next);
    }
}
