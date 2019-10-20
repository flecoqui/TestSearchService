//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace TestWebApp
{
    public partial class Program
    {
        /// <summary>
        /// This is a callback type passed to custom implementaton of windows service state machines.
        /// The callback needs to be called to notify windows about service state changes both when requested to
        /// perform state changes by windows or when they are needed because of other reasons (e.g. unexpected termination).
        /// 
        /// Repeatedly calling this callback also prolonges the default timeout for pending states until the service maanger reports the service as failed.
        /// 
        /// Calling this callback will result in a call to SetServiceStatus - see https://msdn.microsoft.com/en-us/library/windows/desktop/ms686241(v=vs.85).aspx
        /// </summary>
        /// <param name="state">The current state of the service.</param>
        /// <param name="acceptedControlCommands">The currently accepted control commands. E.g. when you set the <paramref name="state"/> to <value>Started</value>, you can indicate that you support pausing and stopping in this state.</param>
        /// <param name="win32ExitCode">The  current win32 exit code. Use this to indicate a failure when setting the state to <value>Stopped</value>.</param>
        /// <param name="waitHint">
        /// The estimeated time in milliseconds until a state changing operation finishes.
        /// For example, you can repeatedly call this callback with <paramref name="state"/> set to <value>StartPending</value> or <value>StopPending</value>
        /// using different values to affect the start/stop progress indicator in service management UI dialogs.
        /// </param>
        public delegate void ServiceStatusReportCallback(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint waitHint);
        internal delegate void ServiceMainFunction(int numArs, IntPtr argPtrPtr);
        private const string ServiceName = "TestWebApp";
        private const string ServiceDescription = "TestWebApp Tool";
        private static ServiceStatus serviceStatus = new ServiceStatus(ServiceType.Win32OwnProcess, ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None,
                        win32ExitCode: 0, serviceSpecificExitCode: 0, checkPoint: 0, waitHint: 0);


        private static string[] ParseArguments(int numArgs, IntPtr argPtrPtr)
        {
            if (numArgs <= 0)
            {
                return Array.Empty<string>();
            }
            // skip first parameter becuase it is the name of the service
            var args = new string[numArgs - 1];
            for (var i = 0; i < numArgs - 1; i++)
            {
                argPtrPtr = IntPtr.Add(argPtrPtr, IntPtr.Size);
                var argPtr = Marshal.PtrToStructure<IntPtr>(argPtrPtr);
                args[i] = Marshal.PtrToStringUni(argPtr);
            }
            return args;
        }


        static public Options ServiceOptions;

        static bool InstallService(Options opt)
        {
            bool bResult = false;

            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        string host = Process.GetCurrentProcess().MainModule.FileName;
                        host += " --service " ;
                        if((opt.UrlList!=null)&& (opt.UrlList.Count>0))
                        {
                            foreach(string url in opt.UrlList)
                            {
                                host += " --url " + url;
                            }
                        }
                        if(!string.IsNullOrEmpty(opt.TraceFile))
                        {
                            host += " --tracefile " + opt.TraceFile;
                            host += " --tracelevel " + opt.TraceLevel.ToString();
                            host += " --tracesize " + opt.TraceSize.ToString();
                        }
                        host += " --consolelevel " + opt.ConsoleLevel.ToString();
                        ServiceHandle h = mgr.CreateService(ServiceName, ServiceDescription, host, ServiceType.Win32OwnProcess, ServiceStartType.AutoStart, ErrorSeverity.Normal, Win32ServiceCredentials.LocalSystem);
                        if (h != null)
                        {

                            bResult = true;
                        }
                        else
                            Console.WriteLine("Install feature: can't create Service: " + ServiceName);
                    }
                    else
                        Console.WriteLine("Install feature: can't open ServiceManager");

                }
                else
                    Console.WriteLine("Install feature: this service is not available on the current platform");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Install feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool UninstallService(Options opt)
        {
            bool bResult = false;
            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {

                            if (Win32ServiceInterop.DeleteService(h) == true)
                            {
                                bResult = true;
                            }
                            else
                                Console.WriteLine("Uninstall feature: can't Uninstall Service: " + ServiceName);
                        }
                        else
                            Console.WriteLine("Uninstall feature: can't open Service: " + ServiceName);
                    }
                    else
                        Console.WriteLine("Uninstall feature: can't open ServiceManager");

                }
                else
                    Console.WriteLine("Uninstall feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Uninstall feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool StartService(Options opt)
        {
            bool bResult = false;
            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {
                            if(Win32ServiceInterop.StartServiceW(h, 0, IntPtr.Zero)==true)
                            {
                                bResult = true;
                            }
                            else
                                Console.WriteLine("Start feature: can't start Service: " + ServiceName);
                        }
                        else
                            Console.WriteLine("Start feature: can't open Service: " + ServiceName);
                    }
                    else
                        Console.WriteLine("Start feature: can't open ServiceManager");
                }
                else
                    Console.WriteLine("Start feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Start feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool StopService(Options opt)
        {
            bool bResult = false;
            try
            { 
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {
                            ServiceStatusProcess status = new ServiceStatusProcess();
                            uint reason = (uint) StopReasonMinorReasonFlags.SERVICE_STOP_REASON_MINOR_MAINTENANCE |
                                (uint) StopReasonMajorReasonFlags.SERVICE_STOP_REASON_MAJOR_NONE |
                                (uint) StopReasonFlags.SERVICE_STOP_REASON_FLAG_UNPLANNED;
                            ServiceStatusParam param = new ServiceStatusParam(reason, status);

                            int s = Marshal.SizeOf<ServiceStatusParam>();
                            var lpParam = Marshal.AllocHGlobal(s);
                            Marshal.StructureToPtr(param, lpParam, fDeleteOld: false);
                            if (Win32ServiceInterop.ControlServiceExW(h,(uint) ServiceControlCommandFlags.SERVICE_CONTROL_STOP,(uint) ServiceControlCommandReasonFlags.SERVICE_CONTROL_STATUS_REASON_INFO, lpParam) == true)
                            {
                                bResult = true;
                            }
                            else
                            {
                                Console.WriteLine("Stop feature: can't stop Service: " + ServiceName + " ErrorCode: " + Marshal.GetLastWin32Error().ToString());
                            }
                        }
                        else
                            Console.WriteLine("Stop feature: can't open Service: " + ServiceName);
                    }
                    else
                        Console.WriteLine("Stop feature: can't open ServiceManager");

                }
                else
                    Console.WriteLine("Stop feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Stop feature: exception: " + ex.Message);
            }
            return bResult;
        }
    }
}
