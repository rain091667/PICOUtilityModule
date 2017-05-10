using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICOUtilityModule;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using MessageHandleUtility;
using MessageHandleUtility.Utility;
using System.Collections.Concurrent;

public enum ConsoleStatusColor
{
    Normal,
    NormalNotice,
    Warning,
    Error
}

namespace EDMPicoDeviceApplication1
{
    class Program
    {
        public static LogManagers LogMessangers;

        private static PICOUtilityManager managers;
        private static PICORawDataListeners ChannelListener;

        // ChannelA Options
        private static readonly Coupling ChannelA_Couping = Coupling.PS5000A_DC;
        private static readonly Range ChannleA_Range = Range.Range_2V;
        private static readonly float ChannleA_Offsets = 0.0f;

        // ChannelB Options
        private static readonly Coupling ChannelB_Couping = Coupling.PS5000A_DC;
        private static readonly Range ChannleB_Range = Range.Range_5V;
        private static readonly float ChannleB_Offsets = 0.0f;

        // Raw Capture Options
        private static readonly uint DataCaptureSampleInterval = 20;
        private static readonly ReportedTimeUnits DataCaptureTimeUnits = ReportedTimeUnits.NanoSeconds;
        private static readonly uint DataCaptureSampleCount = 100000;
        public static short _DeviceMaximumValue = 1;

        // CHA CHB Name
        public static string RootDataFileFolder = string.Empty;

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(CloseEventHandler handler, bool add);
        private delegate void CloseEventHandler();
        static CloseEventHandler _closehandler;

        private static void CloseAll()
        {
            // streaming stop
            //Flags = managers.StopRunStreaming();
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Stop Streaming..." + managers.StopRunStreaming());

            // close pico device
            //Flags = managers.ClosePicoDevice();
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Close Pico Device." + managers.ClosePicoDevice());
        }

        static void Main(string[] args)
        {
            _closehandler += new CloseEventHandler(CloseAll);
            SetConsoleCtrlHandler(_closehandler, true);

            LogMessangers = new LogManagers();

            bool Flags = false;
            managers = new PICOUtilityManager();

            // File Folder
            RootDataFileFolder = Path.Combine(Environment.CurrentDirectory, "" + "Data_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(RootDataFileFolder)) Directory.CreateDirectory(RootDataFileFolder);

            // Default
            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Opening PICO Device.");
            Flags = managers.OpenPicoDevice();
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Opening PICO Device Results: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Fail to Open PICO Device.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            bool setResult = managers.setDeviceResolution(DeviceResolution.PS5000A_DR_15BIT);

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Read PICO Device Driver Version.");
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "PICO Device Driver Version: " + managers.getDeviceInfo(PicoDeviceInfo.PICO_DRIVER_VERSION));

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Read PICO Device Kernel Version.");
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "PICO Device Kernel Version: " + managers.getDeviceInfo(PicoDeviceInfo.PICO_KERNEL_VERSION));

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Prepare and init Channel Listener.");
            ChannelListener = new PICORawDataListeners(RootDataFileFolder);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Channel Listener prepare completed.");

            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Add Trigger Listener. GK3000 Laser Device Add to Listener.");

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Ready to add Raw Data listener to Device. Channel: ChannelA, ChannelB.");
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Raw Data Capture Listener Add to Device. Channel: ChannelA, ChannelB.");
            managers.addChannelRawDataCallBackListener(ChannelListener);

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Ready to add Error listener to Device. Channel: ChannelA, ChannelB.");
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Error Capture Listener Add to Device. Channel: ChannelA, ChannelB.");
            managers.addCaptureErrorListener(ChannelListener);

            // ChannelA, ChannelB
            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Start change ChannelA and ChannelB Options. Channel: ChannelA, ChannelB.");
            // ChannelA
            Flags = managers.ChannelA.setChannelEnable();
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Set ChannelA Enabled. Channel: ChannelA. Result: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Open ChannelA failed. Channel: ChannelA.");
            }
            // ChannelB
            Flags = managers.ChannelB.setChannelEnable();
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Set ChannelB Enabled. Channel: ChannelB. Result: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Open ChannelB failed. Channel: ChannelB.");
            }
            // ChannelA
            Flags = managers.ChannelA.setChannelAction(ChannelA_Couping, ChannleA_Range, ChannleA_Offsets);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice,
                "Change Channel Value: " +
                "Coupling: " + ChannelA_Couping + ", " +
                "Range: " + ChannleA_Range + ", " +
                "Offset: " + ChannleA_Offsets + ". Channel: ChannelA. Result: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Change ChannelA Value failed. Channel: ChannelA.");
            }
            // ChannelB
            Flags = managers.ChannelB.setChannelAction(ChannelB_Couping, ChannleB_Range, ChannleB_Offsets);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice,
                "Change Channel Value: " +
                "Coupling: " + ChannelB_Couping + ", " +
                "Range: " + ChannleB_Range + ", " +
                "Offset: " + ChannleB_Offsets + ". Channel: ChannelB. Result: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Change ChannelB Value failed. Channel: ChannelB.");
            }

            Flags = managers.setChannelBuffers(1024 * 100);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Change ChannelA Buffer Size. Channel: ChannelA. Result: " + Flags);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Change ChannelB Buffer Size. Channel: ChannelB. Result: " + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Change ChannelA Buffer Size failed. Channel: ChannelA.");
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Change ChannelB Buffer Size failed. Channel: ChannelB.");
            }

            // Data Captures
            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Start change Raw Data Capture Options.");

            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Set DataCaptureSampleInterval: " + DataCaptureSampleInterval);
            managers.setDataCaptureSampleInterval(DataCaptureSampleInterval);

            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Set DataCaptureTimeUnits: " + DataCaptureTimeUnits);
            managers.setDataCaptureTimeUnits(DataCaptureTimeUnits);

            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Set DataCaptureSampleCount: " + DataCaptureSampleCount);
            managers.setDataCaptureSampleCount(DataCaptureSampleCount);

            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Read Data Capture Info.");
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DataCaptureSampleCount: " + managers.DataCaptureSampleCount);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DataCaptureSampleInterval: " + managers.DataCaptureSampleInterval);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DataCaptureTimeUnits: " + managers.DataCaptureTimeUnits);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DeviceChannelCountValue: " + managers.DeviceChannelCountValue);
            _DeviceMaximumValue = managers.DeviceMaximumValue;
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DeviceMaximumValue: " + _DeviceMaximumValue);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DevicePowerSupplyStatus: " + managers.DevicePowerSupplyStatus);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "DeviceResolutionValue: " + managers.DeviceResolutionValue);

            // streaming start
            LogMessangers.ConsoleMessage(ConsoleStatusColor.Normal, "Ready to Streaming...");
            Flags = managers.StartRunStreaming(false);
            LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Start Streaming..." + Flags);
            if (!Flags)
            {
                LogMessangers.ConsoleMessage(ConsoleStatusColor.Error, "Fail to start Streaming.");
            }

            // streaming stop
            //Flags = managers.StopRunStreaming();
            //LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Stop Streaming..." + Flags);

            // close pico device
            //Flags = managers.ClosePicoDevice();
            //LogMessangers.ConsoleMessage(ConsoleStatusColor.NormalNotice, "Close Pico Device." + Flags);
        }
    }

    class PICORawDataListeners : DataCallBackCaptureInterface
    {
        // File reset
        private static readonly int ResetFileLengthThreshold = 1000000;

        private string Current_FilePath = string.Empty;
        private TextWriter RawDataWriter = null;
        private int ResetFileCounter = 0;
        private int RawDataLengthCount = 0;
        private int CurrentRawDataArrayLength;
        private bool KeepAlive = false;

        // Events
        private bool CaptureRawDataFlags = true;
        private static string TimeStampRootFolders = string.Empty;
        private string tempWriteData = string.Empty;

        public PICORawDataListeners(string mTimeStampRootFolders)
        {
            TimeStampRootFolders = mTimeStampRootFolders;
            KeepAlive = true;
            ResetFileCounter = 0;
            RawDataLengthCount = 0;
            ResetRawDataFile();
        }

        public void ResetRawDataFile()
        {
            if (!KeepAlive) return;
            RawDataLengthCount = 0;
            if (RawDataWriter == null)
            {
                ResetFileCounter++;
                Current_FilePath = Path.Combine(TimeStampRootFolders, "DATA" + "_" + ResetFileCounter + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv");
                RawDataWriter = new StreamWriter(Current_FilePath, false);
                RawDataWriter.WriteLine("MaxRawData,MinRawData,LaserData");
            }
            else
            {
                RawDataWriter.Flush();
                RawDataWriter.Close();
                RawDataWriter = null;
                ResetFileCounter++;
                Current_FilePath = Path.Combine(TimeStampRootFolders, "DATA" + "_" + ResetFileCounter + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv");
                RawDataWriter = new StreamWriter(Current_FilePath, false);
                RawDataWriter.WriteLine("MaxRawData,MinRawData,LaserData");
            }
        }

        public void StopFileWrites()
        {
            KeepAlive = false;
            RawDataWriter.Flush();
            RawDataWriter.Close();
            RawDataWriter = null;
        }

        public void SetCaptureRawDataFlags(bool flags, string data)
        {
            CaptureRawDataFlags = flags;
        }

        public void OnCaptureError(uint DeviceErrorCode)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss") + "] PICO Error Code: " + DeviceErrorCode);
        }
        public void OnCaptureReceive(RawDataCallback RawData)
        {

            if (!KeepAlive) return;
            CurrentRawDataArrayLength = RawData.ChannelA_RawData_Max.Length;
            for (int i = 0; i < CurrentRawDataArrayLength; i++)
            {
                tempWriteData = "" + adc_to_mv(RawData.ChannelA_RawData_Max[i], RangeInputValue.Range_2V) + "," + adc_to_mv(RawData.ChannelB_RawData_Max[i], RangeInputValue.Range_5V);
                if (tempWriteData[tempWriteData.Length - 1] != '\n') tempWriteData += "\n";
                RawDataWriter.Write(tempWriteData);
            }
            RawDataLengthCount += CurrentRawDataArrayLength;

            if (RawDataLengthCount >= ResetFileLengthThreshold) ResetRawDataFile();
        }

        private int adc_to_mv(int rawdata, ushort InputVlaue)
        {
            return (rawdata * InputVlaue) / Program._DeviceMaximumValue;
        }
    }

    public class LogManagers
    {
        private readonly string LogFiles = "LogFile.txt";
        private StreamWriter LogFileWriter;
        public LogManagers()
        {
            LogFileWriter = new StreamWriter(new BufferedStream(File.Open(LogFiles, FileMode.Append, FileAccess.Write, FileShare.Read)));
        }

        public void ConsoleMessage(ConsoleStatusColor Levels, string msg)
        {
            string CurrentTimes = string.Empty;
            if (Levels == ConsoleStatusColor.Normal)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (Levels == ConsoleStatusColor.NormalNotice)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (Levels == ConsoleStatusColor.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (Levels == ConsoleStatusColor.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            CurrentTimes = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss");
            Console.WriteLine("[" + CurrentTimes + "] " + msg);
            LogFileWriter.WriteLine("[" + CurrentTimes + "] " + msg);
            LogFileWriter.Flush();
        }
    }
}
