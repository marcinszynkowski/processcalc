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

            if (args.Length == 0) // checking for parameters
            {
                log.ToLogAfterError("No user name.");
                Console.WriteLine("Error : No user name.");
                log.ToLogAfterError("Program ended with error code 2.");
                Environment.Exit(2);
            }

            log.ToLogWriteSth("ProcessCalc started working.");

            String username = args[0]; // user name got from the parameter
            
            Console.WriteLine("Checking user : "  + username);
            log.ToLogWriteSth("Checking user : " + username);

            int number = Int32.Parse(ConfigurationManager.AppSettings[username]); // a number from the config file

            Process[] pname = Process.GetProcessesByName(processToCalculate);
            IEnumerable<Process> pnam = pname.OrderBy(pnames => pnames.StartTime);
            List<Process> UserProcess = new List<Process>();
            int procNumb = 0; // number of processes ran by the user
            // Console.WriteLine("\nAll processess of {0} :", processToCalculate);

            log.ToLogWriteSth("\nAll processess of : " + processToCalculate);
            foreach (Process p in pname)
            {
                string id = p.Id + ""; // getting an Id from the process in the table pname
                string own = GetProcessUser(id); // string own = GetProcessUser(id); // getting an owner of running process 
                log.ToLogWriteSth(p.Id + " " + processToCalculate + " " + GetProcessUser((p.Id).ToString()) + " " + p.StartTime);
                if (own.Equals(username))
                {
                    procNumb++;
                }
            }

            if (procNumb > number) // checking a number of processess and deciding about errorcode
            {
                Console.WriteLine("User " + username + " have ran " + procNumb + " processes - " + "user exceeded the limit. ");
                log.ToLogAfterError("User " + username + " have ran " + procNumb + " processes - " + "user exceeded the limit. ");
                Console.WriteLine("The limit is " + number + ". Killing " + (procNumb - number) + " processes.");
                log.ToLogWriteSth("The limit is " + number + ". Killing  " + (procNumb-number) + "  processes.");
                foreach (Process p in pnam)
                {
                    // Console.WriteLine(p.Id + " " + processToCalculate + " " + GetProcessUser((p.Id).ToString()) + " " + p.StartTime);
                    UserProcess.Add(p);
                    if (UserProcess.Count > number)
                    {
                        string id = p.Id + ""; // getting an Id from the process in the table pname
                        string own = GetProcessUser(id); // string own = GetProcessUser(id); // getting an owner of running process 
                        p.Kill();
                        log.ToLogKilledProcessInfo(id, processToCalculate, own);
                    }
                }
                log.ToLogAfterError("Program ended with errorcode 1.");
                Environment.Exit(1);
            }

            else
            {
                log.ToLogAfterError("\nUser " + username + " have ran " + procNumb + " processes");
                log.ToLogAfterError("Program ended with errorcode 0.");
                Environment.Exit(0);
            }
        }
    }
}
