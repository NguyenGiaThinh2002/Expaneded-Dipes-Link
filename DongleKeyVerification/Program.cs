using System.Threading;
using System;
using Securedongle;
using DongleKeyVerification.Extensions;
using System.Text;
using System.IO;
using DongleKeyVerification.Models;

namespace DongleKeyVerification
{
    internal class Program
    {
        #region Variables
        private static USBKey _USBKey = new USBKey();
        private static uint _HardwareIDUsing = 0;
        private static int _levelNumber;
        private const int _maxStationLicense = 4;
        public enum SDCmd : ushort
        {
            SD_FIND = 1,        //Find SecureDongle
            SD_FIND_NEXT = 2,       //Find next
            SD_OPEN = 3,            //Open SecureDongle
            SD_CLOSE = 4,           //Close SecureDongle
            SD_READ,            //Read SecureDongle
            SD_WRITE,           //Write SecureDongle
            SD_RANDOM,          //Generate random
            SD_SEED,            //Generate seed
            SD_DECREASE = 17,
            SD_WRITE_USERID = 9,    //Read UID
            SD_READ_USERID = 10,    //Read UID
            SD_SET_MODULE = 11,   //Set Module
            SD_CHECK_MODULE = 12,   //Check Module
            SD_CALCULATE1 = 14, //Calculate1
            SD_CALCULATE2,      //Calculate1
            SD_CALCULATE3,      //Calculate1
            SD_SET_COUNTER_EX = 160,         //Set Counter, Type change from WORD to DWORD
            SD_GET_COUNTER_EX = 161,          //Read counter, Type change from WORD to DWORD
            SD_SET_TIMER_EX = 162,         //Set Timer Unit Clock, Type change from WORD to DWORD
            SD_GET_TIMER_EX = 163,        //Get Timer Unit Code, , Type change from WORD to DWORD
            SD_ADJUST_TIMER_EX = 164,
        }
        private static string PathProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string PathProgramDataApp => PathProgramData + "\\DP-Link\\"; // C:\ProgramData\R-Link
        public static string PathAllowPC => PathProgramDataApp + "DPConfig";
        #endregion

        static void Main(string[] args)
        {
         
            int keyLevel = 0;
            try
            {
                if (AutoCheckByPassLicenseFile())
                {
                    keyLevel = 4;
                }
                else
                {
                    keyLevel = DetectUSBDongleLevel();
                }
                var client = new DongleKeyNamedPipeHelper();
                client.SendKeyLevel(keyLevel >= 1 ? keyLevel : 0);

                while (true)
                {
                    int newkeyLevel = 0;
                    Thread.Sleep(5000); // Check every 5 seconds
                    if (AutoCheckByPassLicenseFile())
                    {
                        newkeyLevel = 4;
                    }
                    else
                    {
                        newkeyLevel = DetectUSBDongleLevel();
                    }
                
                    if (newkeyLevel != keyLevel)
                    {
                        var newClient = new DongleKeyNamedPipeHelper();
#if DEBUG
                        Console.WriteLine($"Number Station Allow: {newkeyLevel}");
#endif 
                        newClient.SendKeyLevel(newkeyLevel >= 1 ? newkeyLevel : 0);
                        keyLevel = newkeyLevel;
                    }
                }
            }
            catch (Exception)
            {
               
            }

        }

        private static bool AutoCheckByPassLicenseFile()
        {
            // Check HwId PC
            try
            {
                var isByPass = false;
                var uuid = DecryptionHardwareID.GetUniqueID(); // Get PC UUID
                var keyByte = Encoding.UTF8.GetBytes(uuid); // Convert UUID to bytes
                var pathKey = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Dipes-Link"; // Create Folder in app data
                if (!Directory.Exists(pathKey))
                {
                    Directory.CreateDirectory(pathKey);
                }
                File.WriteAllBytes(pathKey + "\\UUID.txt", keyByte); // Save key to file use for bypass

                string pathDPconfig = PathAllowPC + "\\RConfig.dat";
              //  Console.WriteLine($"Checking path: {pathDPconfig}"); // Debugging statement
                if (!Directory.Exists(PathAllowPC))
                {
                    Directory.CreateDirectory(PathAllowPC);
                }
                DecryptionHardwareID.DecryptFile_UUID(pathDPconfig); // Descript file .dat
                foreach (HardwareIDModel id in DecryptionHardwareID.listPCAllow) // Compare byte in descript data
                {
                    var foundKeyByte = Encoding.UTF8.GetBytes(id.HardwareID);
                    for (int i = 0; i < foundKeyByte.Length; i++)
                    {
                        isByPass = true; // Allow if complete compare key
                        if (foundKeyByte[i] != keyByte[i] || foundKeyByte.Length != keyByte.Length)
                        {
                            isByPass = false;
                            break;
                        }
                    }
                }
                DecryptionHardwareID.listPCAllow.Clear();
                return isByPass;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static int DetectUSBDongleLevel()
        {
            for (int level = 1; level <= _maxStationLicense; level++)
            {
                InitVariableUSBDongle(level);
                if (CheckForValidUSBDongleKey())
                {
                    return level;
                }
            }
            return 0;
        }

        private static void InitVariableUSBDongle(int securityLevel)
        {
            InitDongleLevel(securityLevel);
        }

        // Level = allow station 
        private static void InitDongleLevel(int level)
        {
            _USBKey = new USBKey();
            switch (level)
            {
                case 1:
                    _USBKey = new USBKey
                    {
                        USBPassword = new ushort[] { 0xFFFF, 0xEEEE, 0xDDDD, 0xCCCC },
                        InputValue = new ushort[] { 0x50, 0x60, 0x70, 0x80 }
                    };
                    break;
                case 2:
                    _USBKey = new USBKey
                    {
                        USBPassword = new ushort[] { 0x1234, 0x5678, 0x9ABC, 0xDEF0 },
                        InputValue = new ushort[] { 0x01, 0x02, 0x03, 0x04 }
                    };
                    break;
                case 3:
                    _USBKey = new USBKey
                    {
                        USBPassword = new ushort[] { 0xAAAA, 0xBBBB, 0xCCCC, 0xDDDD },
                        InputValue = new ushort[] { 0x10, 0x20, 0x30, 0x40 }
                    };
                    break;
                case 4:
                    _USBKey = new USBKey
                    {
                        USBPassword = new ushort[] { 0x890F, 0x1A74, 0x9844, 0xC60A },
                        InputValue = new ushort[] { 0x06, 0x02, 0x08, 0x15 }
                    };
                    break;
                default:
                    break;
            }
            
            _USBKey.ExpectedResult = CalculateValueWithFormulaDefined(_USBKey.InputValue[0], _USBKey.InputValue[1], _USBKey.InputValue[2], _USBKey.InputValue[3]);
        }

        private static ushort[] CalculateValueWithFormulaDefined(ushort ValueA, ushort ValueB, ushort ValueC, ushort ValueD)
        {
            ValueB = (ushort)(ValueB & ValueD);
            ValueA = (ushort)(ValueA + ValueB);
            ValueC = (ushort)(ValueC - ValueA);
            ValueD = (ushort)(ValueD | ValueC);
            return new ushort[] { ValueA, ValueB, ValueC, ValueD };
        }

        private static bool CheckForValidUSBDongleKey()
        {
            byte[] buffer = new byte[1024];
            uint hardwareID = 0;
            ushort handle = 0;
            uint lp1 = 0;
            uint lp2 = 0;
            ulong ret = 1;
            SecuredongleControl SD = new SecuredongleControl();
            ret = SD.SecureDongle((ushort)SDCmd.SD_FIND, ref handle, ref lp1, ref lp2,
                ref _USBKey.USBPassword[0], ref _USBKey.USBPassword[1], ref _USBKey.USBPassword[2], ref _USBKey.USBPassword[3], buffer);
            if (ret != 0)
            {
                return false;
            }
            hardwareID = lp1;
            if (_HardwareIDUsing == 0)
            {
                _HardwareIDUsing = hardwareID;
            }

            if (_HardwareIDUsing != hardwareID)
            {
                return false;
            }

            ret = SD.SecureDongle((ushort)SDCmd.SD_OPEN, ref handle, ref hardwareID, ref lp2,
                ref _USBKey.USBPassword[0], ref _USBKey.USBPassword[1], ref _USBKey.USBPassword[2], ref _USBKey.USBPassword[3], buffer);
            if (ret != 0)
            {
                return false;
            }
            lp1 = 0;
            lp2 = 0;
            ushort[] inputValue = new ushort[] { _USBKey.InputValue[0], _USBKey.InputValue[1], _USBKey.InputValue[2], _USBKey.InputValue[3] };
            ret = SD.SecureDongle((ushort)SDCmd.SD_CALCULATE1, ref handle, ref lp1, ref lp2,
                ref inputValue[0], ref inputValue[1], ref inputValue[2], ref inputValue[3], buffer);
            if (ret != 0)
            {
                return false;
            }
            for (int i = 0; i < inputValue.Length; i++)
            {
                if (inputValue[i] != _USBKey.ExpectedResult[i])
                {
                    return false;
                }
            }
            ushort p1 = 500;  //Offset of UDZ (UDZ memory position)
            //[500] Rynan 0x54
            //[501] 0x01 Basler camera, 0x02 Cognex camera
            //[502] support level
            //[503] 
            ushort p2 = 4;
            ushort p3 = 0;
            ushort p4 = 0;
            ret = SD.SecureDongle((ushort)SDCmd.SD_READ, ref handle, ref hardwareID, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);
            if (ret != 0)
            {
                return false;
            }

            //Check OEM code
            if (buffer[0] != 0x54) //0x54 is label Rynan
            {
                return false;
            }

            // Check Cognex
            if (buffer[1] != 0x02)
            {
                return false;
            }

            //Close SecureDongle
            SD.SecureDongle((ushort)SDCmd.SD_CLOSE,
                ref handle,
                ref lp1,
                ref lp2,
                ref _USBKey.USBPassword[0],
                ref _USBKey.USBPassword[1],
                ref _USBKey.USBPassword[2],
                ref _USBKey.USBPassword[3],
                buffer);
            return true;
        }

    }
}
    
    


