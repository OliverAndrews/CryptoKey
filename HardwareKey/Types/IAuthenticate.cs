namespace HardwareKey.Types
{
    public interface IAuthenticate<T>
    {
        void AuthenticateThen(IAuthTask next);
    }
}
