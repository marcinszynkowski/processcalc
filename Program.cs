/**
 * Process Calculator
 * Calculates a number of processess ran by user, shows this number
 * abd checks if it exceeds the limits written in the App.config
 * ERROR CODES :
 * 0 - No error (limit is not excedeed)
 * 1 -  too much processes
 * 2 -  no user given :)
**/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

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
            Log log = new Log(); // object of the Log class

            if (args.Length == 0)
            {
                log.ToLogAfterError("No user name.");
                Console.WriteLine("Error : No user name.");
                Environment.Exit(2);
            }

            String username = args[0]; // user name got from the parameter

            Console.WriteLine("Checking user : "  + username);
            log.ToLogWriteSth("Checking user : " + username);
            Process[] pname = Process.GetProcessesByName(processToCalculate);
            IEnumerable<Process> pnam = pname.OrderBy(pnames => pnames.StartTime);
            
            int procNumb = 0; // number of processes
            Console.WriteLine("\nAll processess of {0} :", processToCalculate);
            for (int i = 0; i < pname.Length; i++)
            {
                string id = pname[i].Id + ""; // getting an Id from the process in the table pname
                string own = GetProcessUser(id); // getting an owner of running process
                Console.WriteLine(pname[i].Id + " " + processToCalculate + " " + own);
                if (own.Equals(username))
                {
                    procNumb++;
                }
            }

            Console.WriteLine("\nUser " + username +" have ran " + procNumb + " processes");
            int number = Int32.Parse(ConfigurationManager.AppSettings[username]); // a number from the config file
            if (procNumb > number) // checking a number of processess and deciding about errorcode
            {
                Console.WriteLine("\nUser have ran too many processess.");
                log.ToLogAfterError("User have ran too many processess.");
                Console.WriteLine("Program ended with errorcode 1.");
                //foreach (Process p in pnam)
                //{
                //    int i = pnam.Count();
                //    int limit = procNumb - number;
                //    if (i <= limit)
                //    {
                //        string id = (pnam.ElementAt(i).Id).ToString();
                //        string owner = GetProcessUser(id);
                //        string ProcessName = pnam.ElementAt(i).ProcessName;
                //        log.ToLogKilledProcessInfo(id, ProcessName, owner);
                //    }
                //}
            }
            else
            {
                log.ToLogAfterError("Program ended with errorcode 0.");
                Console.WriteLine("Program ended with errorcode 0.");
                Environment.Exit(0);
            }
        }
    }
}
