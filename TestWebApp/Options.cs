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
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace TestWebApp
{
    [DataContract(Name = "Options")]
    /// <summary>
    /// Options that control the behavior of the program
    /// </summary>
    public class Options
    {
        public Options()
        {
            this.TraceFile = string.Empty;
            this.TraceSize = 524280;
            this.TraceLevel = LogLevel.Information;
            this.ConsoleLevel = LogLevel.Information;
            this.UrlList = null;
        }
        [DataContract(Name = "Action")]
        public enum Action {
            [EnumMember]
            None = 0,
            [EnumMember]
            Help,
            [EnumMember]
            Install,
            [EnumMember]
            Uninstall,
            [EnumMember]
            Start,
            [EnumMember]
            Stop,
            [EnumMember]
            Service
        }
        [DataContract(Name = "LogLevel")]
        public enum LogLevel
        {
            [EnumMember]
            None = 0,

            [EnumMember]
            Error,
            [EnumMember]
            Information,
            [EnumMember]
            Warning,
            [EnumMember]
            Verbose
        }
        [DataMember]
        public LogLevel ConsoleLevel { get; set; }
        [DataMember]
        public LogLevel TraceLevel { get; set; }
        [DataMember]
        public string TraceFile { get; set; }
        [DataMember]
        public int TraceSize { get; set; }

        [DataMember]
        public Action TestWebAppAction { get; set; }
        [DataMember]
        public List<string> UrlList { get; set; }

        public bool ServiceMode { get; set; }


        public string GetErrorMessage()
        {
            return ErrorMessage;
        }
        public string GetInformationMessage(Int32 version)
        {
            bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            if(IsWindows)
                return string.Format(InformationMessagePrefix, ASVersion.GetVersionString(version)) +
                   InformationMessageWindows + InformationMessageSuffix ;
            else
                return string.Format(InformationMessagePrefix, ASVersion.GetVersionString(version)) +
                   InformationMessageSuffix;

        }
        public string GetErrorMessagePrefix()
        {
            return ErrorMessagePrefix;
        }
        void LogMessage(LogLevel level, string Message)
        {
            string Text = string.Empty;
            if ((level <= TraceLevel) && (!string.IsNullOrEmpty(this.TraceFile)))
            {
                Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\r\n";
                LogTrace(this.TraceFile, this.TraceSize, Text);
            }
            if (level <= ConsoleLevel)
            {
                if (string.IsNullOrEmpty(Text))
                    Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\r\n";
                Console.Write(Text);
            }
        }
        public void LogVerbose(string Message)
        {
            LogMessage(LogLevel.Verbose, Message);
        }
        public void LogInformation(string Message)
        {
            LogMessage(LogLevel.Information, Message);
        }
        public void LogWarning(string Message)
        {
            LogMessage(LogLevel.Warning, Message);
        }
        public void LogError(string Message)
        {
            LogMessage(LogLevel.Error, Message);
        }
        static public char GetChar(byte b)
        {
            if ((b >= 32) && (b < 127))
                return (char)b;
            return '.';
        }
        static public string DumpHex(byte[] data)
        {
            string result = string.Empty;
            string resultHex = " ";
            string resultASCII = " ";
            int Len = ((data.Length % 16 == 0) ? (data.Length / 16) : (data.Length / 16) + 1) * 16;
            for (int i = 0; i < Len; i++)
            {
                if (i < data.Length)
                {
                    resultASCII += string.Format("{0}", GetChar(data[i]));
                    resultHex += string.Format("{0:X2} ", data[i]);
                }
                else
                {
                    resultASCII += " ";
                    resultHex += "   ";
                }
                if (i % 16 == 15)
                {
                    result += string.Format("{0:X8} ", i - 15) + resultHex + resultASCII + "\r\n";
                    resultHex = " ";
                    resultASCII = " ";
                }
            }
            return result;
        }
        public ulong LogTrace(string fullPath, long Tracefile, string Message)
        {
            ulong retVal = 0;

            try
            {
                lock (this)
                {
                    FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    if (fs != null)
                    {
                        long pos = fs.Seek(0, SeekOrigin.End);
                        byte[] data = UTF8Encoding.UTF8.GetBytes(Message);
                        if (data != null)
                        {
                            if (pos + data.Length > Tracefile)
                                fs.SetLength(0);
                            fs.Write(data, 0, data.Length);
                            retVal = (ulong)data.Length;
                        }
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while append in file:" + fullPath + " Exception: " + ex.Message);
            }
            return retVal;
        }
        static public LogLevel GetLogLevel(string text)
        {
            LogLevel level = LogLevel.None;
            switch (text.ToLower())
            {
                case "none":
                    level = LogLevel.None;
                    break;
                case "information":
                    level = LogLevel.Information;
                    break;
                case "error":
                    level = LogLevel.Error;
                    break;
                case "warning":
                    level = LogLevel.Warning;
                    break;
                case "verbose":
                    level = LogLevel.Verbose;
                    break;
                default:
                    break;
            }
            return level;
        }
        public static object ReadObjectByType(string filepath, Type type)
        {
            object retVal = null;
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                    retVal = ser.ReadObject(fs);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while reading config file: " + ex.Message);

            }
            return retVal;
        }
        public static bool WriteObjectByType(string filepath, Type type, object obj)
        {
            bool retVal = false;
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (fs != null)
                {
                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                    ser.WriteObject(fs, obj);
                    fs.Close();
                    retVal = true;
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while writing config file: " + ex.Message);

            }
            return retVal;
        }
        private readonly string ErrorMessagePrefix = "TestWebApp Error: \r\n";
        private readonly string InformationMessagePrefix = "TestWebApp:\r\n" + "Version: {0} \r\n" + "Syntax:\r\n" +
            "TestWebApp  [--url <url1>] [--url <url2>]  \r\n" +
            "            [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|information|warning|verbose>]\r\n" +
            "            [--consolelevel <none|error|information|warning|verbose>]\r\n";

        private readonly string InformationMessageWindows =
            "TestWebApp --install \r\n" +
            "          [--url <url1>] [--url <url2>]  \r\n" +
            "          [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|information|warning|verbose>]\r\n" +
            "          [--consolelevel <none|error|information|warning|verbose>]\r\n" +
            "TestWebApp --uninstall  \r\n" +
            "TestWebApp --start \r\n" +
            "TestWebApp --stop  \r\n";
        private readonly string InformationMessageSuffix = "TestWebApp --help\r\n";
        private string ErrorMessage = string.Empty;


        public static Options InitializeOptions(string[] args)
        {
            List<Options> list = new List<Options>();
            Options options = new Options();

            if ((options == null)||(list == null))
            {
                return null;
            }
            try
            {
                options.TestWebAppAction = Action.Service;
                options.TraceFile = "TestWebApp.log";
                if (args!=null)
                {

                    int i = 0;
                    if(args.Length == 0)
                    {
                        return options;
                    }
                    while ((i < args.Length)&&(string.IsNullOrEmpty(options.ErrorMessage)))
                    {
                        switch(args[i++])
                        {

                            case "--help":
                                options.TestWebAppAction = Action.Help;
                                break;
                            case "--service":
                                options.ServiceMode = true;
                                options.TestWebAppAction = Action.Service;
                                break;
                            case "--install":
                                options.TestWebAppAction = Action.Install;
                                break;
                            case "--uninstall":
                                options.TestWebAppAction = Action.Uninstall;
                                break;
                            case "--start":
                                options.TestWebAppAction = Action.Start;
                                break;
                            case "--stop":
                                options.TestWebAppAction = Action.Stop;
                                break;
                            case "--url":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    if (options.UrlList == null)
                                        options.UrlList = new List<string>();
                                    options.UrlList.Add(args[i++]);
                                }
                                else
                                    options.ErrorMessage = "url not set";
                                break;
                            case "--tracefile":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.TraceFile = args[i++];
                                else
                                    options.ErrorMessage = "TraceFile not set";
                                break;
                            case "--tracesize":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int tracesize = 0;
                                    if (int.TryParse(args[i++], out tracesize))
                                        options.TraceSize = tracesize;
                                    else
                                        options.ErrorMessage = "TraceSize value incorrect";
                                }
                                else
                                    options.ErrorMessage = "TraceSize not set";
                                break;
                            case "--tracelevel":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.TraceLevel = GetLogLevel(args[i++]);
                                else
                                    options.ErrorMessage = "TraceLevel not set";
                                break;
                            case "--consolelevel":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.ConsoleLevel = GetLogLevel(args[i++]);
                                else
                                    options.ErrorMessage = "ConsoleLevel not set";
                                break;

                            default:
                                if ((args[i - 1].ToLower() == "dotnet") ||
                                    (args[i - 1].ToLower() == "testwebapp.dll") ||
                                    (args[i - 1].ToLower() == "testwebapp.exe"))
                                    break;
                                options.ErrorMessage = "wrong parameter: " + args[i-1];
                                return options;
                        }
                    }

                    if (options.TestWebAppAction == Action.None)
                    {
                        options.ErrorMessage = "No feature in the command line";
                        return options;
                    }
                }
            }
            catch (Exception ex)
            {
                options.ErrorMessage = "Exception while analyzing the options: " + ex.Message;
                return options;
            }

            if (!string.IsNullOrEmpty(options.ErrorMessage))
            {
                return options;
            }
            return CheckOptions(options);

        }
        public static Options CheckOptions(Options options)
        {
            if ((options.TestWebAppAction == Action.Help) ||
                 (options.TestWebAppAction == Action.Uninstall) || 
                 (options.TestWebAppAction == Action.Install) ||
                  (options.TestWebAppAction == Action.Stop) ||
                  (options.TestWebAppAction == Action.Service) ||
                   (options.TestWebAppAction == Action.Start))
            {
                return options;
            }

            return null;
        }

    }
}
