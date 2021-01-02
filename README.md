## Creating a Crypto Key

#### Date: 2021-01-01
---
### Testable Idea

Identifiable elements of a USB drive can be used in conjunction with a hash value to produce a basic hardware key.

### Success Conditions

BASIC
UTILITY
RUNTIME
PROTECTION

Producing a USB drive which, when plugged in, allows for a handshake => BURP handshake to happen, runs some function. I'm picturing a signature being formed from something like the serial number, or other device features.

### Failure Conditions

Failure to find needed information on the USB key to allow a signature to be formed.

#### Partial Failure

Failure to find *immutable* information on the USB key which can be used to form a signature.

### Procedural Notes

Going to build out the code and have the actual encryption system removable.

Just verify that I can get the values and then run some operation. If that operation returns true, then I can go and run some other operation.

I'm using this interface as the one for the fetching the devices.

```csharp
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel();

        bool HasDevicesWithLabel(string name);
    }
```

For validating a device, I'll use this interface:
```csharp
    interface Validate<T>
    {
        T ValidationAlgorithm();

        bool Success();
    }
```

The main action would be to get the devices, then pass that into the validator. That will have both of the above as a dependency. For now I'll just write a class, then make it more abstract later. I think the two things that are going to move the most are the implementation of the validation algorithm. I will also do more of a skunk works approach to create a class, tinker with that, then create the interface. I'm keeping everything compacted into [[Region]] blocks.

```csharp
My final interfaces are:

    #region Interfaces
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel();

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
	
```

My final class which needs later abstraction is:
```csharp
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
```

Now I can take the classes which I wrote for each of the IValidate and the Certificates. I am just using the T for certificate as a single drive. The validate drive will run on that, and tell the authenticator if it is able to move on to the next step, which is to run a class that is provided.

Now I've just defined a simple task to run after the authentication.

```csharp
    #region Task Definition
    class AuthTest : IAuthTask
    {
        public void Main()
        {
            Console.WriteLine("Done");
        }
    }
    #endregion
```

And I have my validation algorithm just stubbed out.
```csharp
    #region Validator Definition
    class ValidateDrive : IValidate<DriveInfo>
    {
        private bool RunCheckOnDrive(DriveInfo drive)
        {
            var result = false;
            result = true;
            return result;
        }

        public bool Check(DriveInfo drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
    #endregion
```

#### Developing a Validation Scheme
I'll need to determine all the fields in the drive. So I'll explore the properties. Those are defined as follows.
![[Pasted image 20210101230129.png]]

Which gives some useful info. There's also another library which can be used, which is [[ManagementObject]]. If I can find common fields, I can use the drive list to look up this object. Or maybe just use DriveInfo all together. I would only need both in the case where one has fields that the other does not.

The management object implementation looks like this.
```csharp
        public ManagementObject GetDeviceByLabel(string name)
        {
            var search = 
				new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            var results = search.Get();
            return results
                .OfType<ManagementObject>()
                .Where<ManagementObject>(x => true)
                .FirstOrDefault();
        }
```
Clearly that Where operation is stubbed out. That is what I need to define after I compare the two ways. That ManagmentObject is far too large for putting here...

This is what I ended up using. I only used ManagmentObject. The query is finiky.

```csharp

        public ManagementBaseObject GetDeviceByLabel(string name)
        {
            var search = 
				new ManagementObjectSearcher(
					$"SELECT * FROM Win32_LogicalDisk");
            var results = search.Get();
            return results
				.OfType<ManagementBaseObject>()
                .Where(x => x["VolumeName"].ToString() == name)
                .FirstOrDefault();
        }
```

The main object worked out to look something like this:

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = 
				new AuthenticateHardware<ManagementBaseObject>(
					new ValidateDrive(), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }
```

Now I have two things that I can work with. I can use the serial number as a unique identifier for the drive. I can then put a file somewhere on the drive with a specific name. This can then be used along with a password to run some operation.

- Use an [[RSA Key]]

The next thing to do is to write up the validator. I will want to come up with something that would be tamper proof by design. Attach the serial number to the password and the file. Do something with the serial number.
- I think for now I'll just use a basic [[Symmetric Key]] system.

My key will be something which, when unlocked, exposes the hash of the USB key's serial number, which is then checked against the serial number on the device. If the contents of the encrypted string matches the serial number of the device, then it is considered validated. This is what I am using to do the authentication.

```csharp
    #region Validator Definition
    class ValidateDrive : IValidate<ManagementBaseObject>
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
                var hash = md5.ComputeHash(Encoding
					.UTF8
					.GetBytes(target));
                stringHash = BitConverter
					.ToString(hash)
					.Replace("-", "");
            }
            return stringHash;
        }

        private bool RunCheckOnDrive(ManagementBaseObject drive)
        {
            var checkHash = CreateHash(drive["VolumeSerialNumber"]
				.ToString());
            return (checkHash == _expected);
        }

        public bool Check(ManagementBaseObject drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
```

The final application main method worked out to this:
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = 
			new AuthenticateHardware<ManagementBaseObject>(
				new ValidateDrive("42E7D729"), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }
```
### Results
I was able to produce a simple password out of the serial number. Therefore there is at least a partial success here. To prove full success, I'd need to demonstrate that the serial number is not something I can duplicate. I would also want to add more robust checking.

[Code](https://github.com/OliverAndrews/CryptoKey)## Creating a Crypto Key

#### Date: 2021-01-01
---
### Testable Idea

Identifiable elements of a USB drive can be used in conjunction with a hash value to produce a basic hardware key.

### Success Conditions

BASIC
UTILITY
RUNTIME
PROTECTION

Producing a USB drive which, when plugged in, allows for a handshake => BURP handshake to happen, runs some function. I'm picturing a signature being formed from something like the serial number, or other device features.

### Failure Conditions

Failure to find needed information on the USB key to allow a signature to be formed.

#### Partial Failure

Failure to find *immutable* information on the USB key which can be used to form a signature.

### Procedural Notes

Going to build out the code and have the actual encryption system removable.

Just verify that I can get the values and then run some operation. If that operation returns true, then I can go and run some other operation.

I'm using this interface as the one for the fetching the devices.

```csharp
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel();

        bool HasDevicesWithLabel(string name);
    }
```

For validating a device, I'll use this interface:
```csharp
    interface Validate<T>
    {
        T ValidationAlgorithm();

        bool Success();
    }
```

The main action would be to get the devices, then pass that into the validator. That will have both of the above as a dependency. For now I'll just write a class, then make it more abstract later. I think the two things that are going to move the most are the implementation of the validation algorithm. I will also do more of a skunk works approach to create a class, tinker with that, then create the interface. I'm keeping everything compacted into [[Region]] blocks.

```csharp
My final interfaces are:

    #region Interfaces
    interface IDriveFinder<T>
    {
        T GetDeviceByLabel(string name);

        bool HasDeviceOfName(string name);

        IEnumerable<T> GetDevicesWithLabel();

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
	
```

My final class which needs later abstraction is:
```csharp
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
```

Now I can take the classes which I wrote for each of the IValidate and the Certificates. I am just using the T for certificate as a single drive. The validate drive will run on that, and tell the authenticator if it is able to move on to the next step, which is to run a class that is provided.

Now I've just defined a simple task to run after the authentication.

```csharp
    #region Task Definition
    class AuthTest : IAuthTask
    {
        public void Main()
        {
            Console.WriteLine("Done");
        }
    }
    #endregion
```

And I have my validation algorithm just stubbed out.
```csharp
    #region Validator Definition
    class ValidateDrive : IValidate<DriveInfo>
    {
        private bool RunCheckOnDrive(DriveInfo drive)
        {
            var result = false;
            result = true;
            return result;
        }

        public bool Check(DriveInfo drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
    #endregion
```

#### Developing a Validation Scheme
I'll need to determine all the fields in the drive. So I'll explore the properties. Those are defined as follows.
![[Pasted image 20210101230129.png]]

Which gives some useful info. There's also another library which can be used, which is [[ManagementObject]]. If I can find common fields, I can use the drive list to look up this object. Or maybe just use DriveInfo all together. I would only need both in the case where one has fields that the other does not.

The management object implementation looks like this.
```csharp
        public ManagementObject GetDeviceByLabel(string name)
        {
            var search = 
				new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            var results = search.Get();
            return results
                .OfType<ManagementObject>()
                .Where<ManagementObject>(x => true)
                .FirstOrDefault();
        }
```
Clearly that Where operation is stubbed out. That is what I need to define after I compare the two ways. That ManagmentObject is far too large for putting here...

This is what I ended up using. I only used ManagmentObject. The query is finiky.

```csharp

        public ManagementBaseObject GetDeviceByLabel(string name)
        {
            var search = 
				new ManagementObjectSearcher(
					$"SELECT * FROM Win32_LogicalDisk");
            var results = search.Get();
            return results
				.OfType<ManagementBaseObject>()
                .Where(x => x["VolumeName"].ToString() == name)
                .FirstOrDefault();
        }
```

The main object worked out to look something like this:

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = 
				new AuthenticateHardware<ManagementBaseObject>(
					new ValidateDrive(), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }
```

Now I have two things that I can work with. I can use the serial number as a unique identifier for the drive. I can then put a file somewhere on the drive with a specific name. This can then be used along with a password to run some operation.

- Use an [[RSA Key]]

The next thing to do is to write up the validator. I will want to come up with something that would be tamper proof by design. Attach the serial number to the password and the file. Do something with the serial number.
- I think for now I'll just use a basic [[Symmetric Key]] system.

My key will be something which, when unlocked, exposes the hash of the USB key's serial number, which is then checked against the serial number on the device. If the contents of the encrypted string matches the serial number of the device, then it is considered validated. This is what I am using to do the authentication.

```csharp
    #region Validator Definition
    class ValidateDrive : IValidate<ManagementBaseObject>
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
                var hash = md5.ComputeHash(Encoding
					.UTF8
					.GetBytes(target));
                stringHash = BitConverter
					.ToString(hash)
					.Replace("-", "");
            }
            return stringHash;
        }

        private bool RunCheckOnDrive(ManagementBaseObject drive)
        {
            var checkHash = CreateHash(drive["VolumeSerialNumber"]
				.ToString());
            return (checkHash == _expected);
        }

        public bool Check(ManagementBaseObject drive)
        {
            return RunCheckOnDrive(drive);
        }

    }
```

The final application main method worked out to this:
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var finder = new ManagerDeviceFinder();
            var result = finder.GetDeviceByLabel("TESTDISK");
            var auth = 
			new AuthenticateHardware<ManagementBaseObject>(
				new ValidateDrive("42E7D729"), result);
            auth.AuthenticateThen(new AuthTest());
        }
    }
```
### Results
I was able to produce a simple password out of the serial number. Therefore there is at least a partial success here. To prove full success, I'd need to demonstrate that the serial number is not something I can duplicate. I would also want to add more robust checking.

[Code](https://github.com/OliverAndrews/CryptoKey)
