using System;
using System.Runtime.InteropServices;   //GuidAttribute
using System.Reflection;                //Assembly
using System.Threading;                 //Mutex
using System.Security.AccessControl;    //MutexAccessRule
using System.Security.Principal;        //SecurityIdentifier
using System.Diagnostics;

namespace Lifeguard
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static String LifeguardMainMutexGuid = "C7FCF167-4792-4FAF-ACBF-D9F0BB3356F9";
        private static String lifeguardAppName = "LifeguardBackgroundApp.exe";

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start() {
            //quick and dirty window hider
            Process p = Process.GetCurrentProcess();
            if (p != null)
            {
                IntPtr handle = p.MainWindowHandle;

                if (handle != IntPtr.Zero)
                    ShowWindow(handle, 0);
            }

            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                // edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        // mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process, it will still get acquired
                        hasHandle = true;
                    }

                    // Perform your work here.
                    if (hasHandle)
                    {
                        while (true)
                        {
                            var isMainRunning = isMainProcessRunning();
                
                            if (!isMainRunning) {
                                //restart main app
                                try
                                {
                                    var exePath = Assembly.GetExecutingAssembly().Location;
                                    var folder = System.IO.Path.GetDirectoryName(exePath);

                                    var process = new Process();
                                    process.StartInfo.FileName = System.IO.Path.Combine(folder, lifeguardAppName);
                                    process.Start();
                                }
                                catch (Exception e) {
                                    Console.WriteLine(e.GetType() + "  " + e.Message);
                                }
                            }
                            Thread.Sleep(30000);
                        }
                    }
                }
                catch (Exception e) {
                }
                finally
                {
                    Console.WriteLine("Done, releasing mutex? " + hasHandle);
                    Thread.Sleep(2000);
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        private static bool isMainProcessRunning()
        {
            var hasHandle = false;
            string mutexId = string.Format("Global\\{{{0}}}", LifeguardMainMutexGuid);

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                // edited by acidzombie24
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        // mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process, it will still get acquired
                        hasHandle = true;
                    }
                }
                catch (Exception e)
                {
                }
                finally
                {
                    Thread.Sleep(2000);
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
            return !hasHandle;
        }
    }


}
