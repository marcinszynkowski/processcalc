/**
 * calculating a number of processess ran by user and showing this number
 * checking if it exceeds the limits
**/

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
namespace ProcessCalc
{
    class Program
    {
        private static string processToCalculate = ConfigurationManager.AppSettings["ProcessToCalculate"];

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        
        // a method for getting a process owner by process
        public static string GetProcessUser(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        // a method for getting a process owner by processId
        private static string GetProcessUser(string processId)
        {
            ManagementObjectSearcher proc = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + processId);

            string owner = null;
            string[] OwnerInfo = new string[2];

            foreach (ManagementObject p in proc.Get())
            {
                p.InvokeMethod("GetOwner", (object[])OwnerInfo);
                owner = OwnerInfo[0].ToString();
            }
            return owner;
        }

        static void Main(string[] args) 
        {
            String username = args[0]; // user name got from the parameter

            Console.WriteLine("Checking user : "  + username);
            Process[] pname = Process.GetProcessesByName(processToCalculate);
            int procNumb = 0; // number of processes
            for (int i = 0; i < pname.Length; i++)
            {
                string id = pname[i].Id + ""; // getting an Id from the process in the table
                string own = GetProcessUser(id); // getting an owner of running process
                
                Console.WriteLine(pname[i].Id + " " + own);
                if (own.Equals(username))
                {
                    procNumb++;
                }
            }

            Console.WriteLine("User " + username +" have ran " + procNumb + " processes");
            int number = Int32.Parse(ConfigurationManager.AppSettings[username]); // a number from the config file
            if (procNumb > number) 
            {
                Console.WriteLine("You have ran too many processess.");
                Console.WriteLine("Program ended with errorcode 1.");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Program ended with errorcode 0.");
                Environment.Exit(0);
            }
        }
    }
}
