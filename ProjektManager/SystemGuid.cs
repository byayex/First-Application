using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Security.Cryptography;

namespace ProjektManager
{
    /// <summary>
    /// Generates a Guid based on the current computer hardware
    /// Example: C384B159-8E36-6C85-8ED8-6897486500FF
    /// </summary>
    public class SystemGuid
    {
        public string systemGuid = string.Empty;
        public string Value()
        {
            if (string.IsNullOrEmpty(systemGuid))
            {
                string cpuId = GetCpuId();
                string biodId = GetBiosId();
                string mainboard = GetMainboardId();
                string gpuId = GetGpuId();
                string mac = GetMac();
                string concatStr = $"CPU: {cpuId}\nBIOS:{biodId}\nMainboard: {mainboard}\nGPU: {gpuId}\nMAC: {mac}";
                systemGuid = GetHash(concatStr);
            }
            return systemGuid;
        }
        private string GetHash(string s)
        {
            try
            {
                var lProvider = new MD5CryptoServiceProvider();
                var lUtf8 = lProvider.ComputeHash(ASCIIEncoding.UTF8.GetBytes(s));
                return new Guid(lUtf8).ToString().ToUpper();
            }
            catch (Exception lEx)
            {
                return lEx.Message;
            }
        }

        #region Original Device ID Getting Code

        //Return a hardware identifier
        private string GetIdentifier(string pWmiClass, List<string> pProperties)
        {
            string lResult = string.Empty;
            try
            {
                foreach (ManagementObject lItem in new ManagementClass(pWmiClass).GetInstances())
                {
                    foreach (var lProperty in pProperties)
                    {
                        try
                        {
                            switch (lProperty)
                            {
                                case "MACAddress":
                                    if (string.IsNullOrWhiteSpace(lResult) == false)
                                        return lResult;

                                    if (lItem["IPEnabled"].ToString() != "True")
                                        continue;
                                    break;
                            }

                            var lItemProperty = lItem[lProperty];
                            if (lItemProperty == null)
                                continue;

                            var lValue = lItemProperty.ToString();
                            if (string.IsNullOrWhiteSpace(lValue) == false)
                                lResult += $"{lValue}; ";
                        }
                        catch { }
                    }

                }
            }
            catch { }
            return lResult.TrimEnd(' ', ';');
        }

        private List<string> ListOfCpuProperties = new List<string> { "UniqueId", "ProcessorId", "Name", "Manufacturer" };

        private string GetCpuId()
        {
            return GetIdentifier("Win32_Processor", ListOfCpuProperties);
        }

        private List<string> ListOfBiosProperties = new List<string> { "Manufacturer", "SMBIOSBIOSVersion", "IdentificationCode", "SerialNumber", "ReleaseDate", "Version" };
        //BIOS Identifier
        private string GetBiosId()
        {
            return GetIdentifier("Win32_BIOS", ListOfBiosProperties);
        }

        private List<string> ListOfMainboardProperties = new List<string> { "Model", "Manufacturer", "Name", "SerialNumber" };
        //Motherboard ID
        private string GetMainboardId()
        {
            return GetIdentifier("Win32_BaseBoard", ListOfMainboardProperties);
        }

        private List<string> ListOfGpuProperties = new List<string> { "Name" };
        //Primary video controller ID
        private string GetGpuId()
        {
            return GetIdentifier("Win32_VideoController", ListOfGpuProperties);
        }

        private List<string> ListOfNetworkProperties = new List<string> { "MACAddress" };
        private string GetMac()
        {
            return GetIdentifier("Win32_NetworkAdapterConfiguration", ListOfNetworkProperties);
        }

        #endregion
    }
}