using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareKey.Types
{
    interface IValidate<T>
    {
        bool Check(T certificate);
    }
}
