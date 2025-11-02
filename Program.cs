using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.NetworkInformation;

class Program
{
    static void Main()
    {
        bool isVM = IsVirtualMachine();

        if (isVM)
        {
            MessageBox.Show("Este computador é uma máquina virtual.", "Detecção de VM", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Este computador NÃO é uma máquina virtual.", "Detecção de VM", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    static bool IsVirtualMachine()
    {
        if(CheckVmWmi())
            return true;

        if (CheckVmMac())
            return true;

        if (CheckVmProcesses())
            return true;


        return false;
    }

    static bool CheckVmWmi() // Aqui eh a veriricacao do Manufacturer
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                foreach (var item in searcher.Get())
                {
                    string manufacturer = item["Manufacturer"].ToString().ToLower();
                    string model = item["Model"].ToString().ToLower();

                    if ((manufacturer.Contains("microsoft") && model.Contains("virtual")) ||
                        manufacturer.Contains("vmware") ||
                        model.Contains("virtualbox") ||
                        model.Contains("kvm") ||
                        model.Contains("qemu"))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {

        }

        return false;
    }

    static bool CheckVmMac() // Aqui eh a verificacao do MAC
    {
        try
        {
            string[] vmMacPrefixes = {
            "00-05-69", // VMware
            "00-0C-29", // VMware
            "00-1C-14", // VMware
            "00-50-56", // VMware
            "08-00-27", // VirtualBox
            "00-15-5D"  // Hyper-V
        };

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                string mac = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());

                foreach (string prefix in vmMacPrefixes)
                {
                    if (mac.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {

        }

        return false;
    }

    static bool CheckVmProcesses() // Aqui eh o check dos processos
    {
        // Lista dos processos comuns
        string[] vmProcesses = {
        "vmtoolsd",
        "vmwaretray",
        "vmacthlp",
        "vboxservice",
        "vboxtray",
        "vmcompute",
        "vmms"
        };

        try
        {
            foreach (var process in Process.GetProcesses())
            {
                string processName = process.ProcessName.ToLower();

                foreach (string vmProcess in vmProcesses)
                {
                    if (processName.Contains(vmProcess))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {

        }

        return false;
    }
}
