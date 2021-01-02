using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareKey.Types
{
    public interface IValidate<T>
    {
        bool Check(T certificate);
    }
}
