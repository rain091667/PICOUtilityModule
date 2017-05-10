# PICOUtilityModule

    PicoScope Device (Pico Technology) C# Module
	
# Device Information
* [Pico Technology Device] PicoScope 5000 Series 5243b

<table width=100%>
  <tr>
    <th>PICO Device</th>
    <th>PICO Device</th> 
  </tr>
  <tr>
    <td><img src="https://raw.githubusercontent.com/rain091667/PICOUtilityModule/master/ScreenDemo/Device1.jpg" width="350" height="300" /></td>
    <td><img src="https://raw.githubusercontent.com/rain091667/PICOUtilityModule/master/ScreenDemo/Device2.jpg" width="350" height="300" /></td>
  </tr>
</table>

## PICOUtilityModule DEMO
![](https://raw.githubusercontent.com/rain091667/PICOUtilityModule/master/ScreenDemo/SampleScreen.png)

# Usage

## DLL Reference

# dependency DLL
* picoipp.dll
* ps5000a.dll

```csharp
using PICOUtilityModule;
```

### Implement DataCallBackCaptureInterface
```csharp
class PICORawDataListeners : DataCallBackCaptureInterface
{
	public void OnCaptureError(uint DeviceErrorCode)
	{
		// PICO Device Error Code.
	}
	
	public void OnCaptureReceive(RawDataCallback RawData)
	{
		// Data Callback
		
		// RawData.ChannelA_RawData_Max // ChannelA Max Data.
		// RawData.ChannelA_RawData_Min // ChannelA Min Data.
		
		// RawData.ChannelB_RawData_Max // ChannelB Max Data.
		// RawData.ChannelB_RawData_Min // ChannelB Min Data.
		
		// RawData.ChannelC_RawData_Max // ChannelC Max Data.
		// RawData.ChannelC_RawData_Min // ChannelC Min Data.
		
		// RawData.ChannelD_RawData_Max // ChannelD Max Data.
		// RawData.ChannelD_RawData_Min // ChannelD Min Data.
	}
}
```

## Basic Information
* RangeInputValue
```csharp
	/// <summary>
    /// Pico RangeInputValue.
    /// </summary>
    public class RangeInputValue
    {
        /// <summary>
        /// Range_10mV. Value: 10.
        /// </summary>
        public const ushort Range_10mV = 10;
        /// <summary>
        /// Range_20mV. Value: 20.
        /// </summary>
        public const ushort Range_20mV = 20;
        /// <summary>
        /// Range_50mV. Value: 50.
        /// </summary>
        public const ushort Range_50mV = 50;
        /// <summary>
        /// Range_100mV. Value: 100.
        /// </summary>
        public const ushort Range_100mV = 100;
        /// <summary>
        /// Range_200mV. Value: 200.
        /// </summary>
        public const ushort Range_200mV = 200;
        /// <summary>
        /// Range_500mV. Value: 500.
        /// </summary>
        public const ushort Range_500mV = 500;
        /// <summary>
        /// Range_1V. Value: 1000.
        /// </summary>
        public const ushort Range_1V = 1000;
        /// <summary>
        /// Range_2V. Value: 2000.
        /// </summary>
        public const ushort Range_2V = 2000;
        /// <summary>
        /// Range_5V. Value: 5000.
        /// </summary>
        public const ushort Range_5V = 5000;
        /// <summary>
        /// Range_10V. Value: 10000.
        /// </summary>
        public const ushort Range_10V = 10000;
        /// <summary>
        /// Range_20V. Value: 20000.
        /// </summary>
        public const ushort Range_20V = 20000;
        /// <summary>
        /// Range_50V. Value: 50000.
        /// </summary>
        public const ushort Range_50V = 50000;
    }
```

* PicoDeviceStatus
```csharp
	/// <summary>
    /// Pico device status.
    /// </summary>
    public enum PicoDeviceStatus : uint
    {
        /// <summary>
        /// DeviceOpened.
        /// </summary>
        DeviceOpened,
        /// <summary>
        /// DeviceStreaming.
        /// </summary>
        DeviceStreaming,
        /// <summary>
        /// DeviceClosed.
        /// </summary>
        DeviceClosed
    }
```

* DeviceResolution
```csharp
	/// <summary>
    /// Pico device resolution.
    /// </summary>
    public enum DeviceResolution : uint
    {
        /// <summary>
        /// Device resolution 8bits.
        /// </summary>
        PS5000A_DR_8BIT,
        /// <summary>
        /// Device resolution 12bits.
        /// </summary>
        PS5000A_DR_12BIT,
        /// <summary>
        /// Device resolution 14bits.
        /// </summary>
        PS5000A_DR_14BIT,
        /// <summary>
        /// Device resolution 15bits.
        /// </summary>
        PS5000A_DR_15BIT,
        /// <summary>
        /// Device resolution 16bits.
        /// </summary>
        PS5000A_DR_16BIT
    }
```

* Range
```csharp
	/// <summary>
    /// Pico device range.
    /// </summary>
    public enum Range : uint
    {
        /// <summary>
        /// Device range 10mV.
        /// </summary>
        Range_10mV,
        /// <summary>
        /// Device range 20mV.
        /// </summary>
        Range_20mV,
        /// <summary>
        /// Device range 50mV.
        /// </summary>
        Range_50mV,
        /// <summary>
        /// Device range 100mV.
        /// </summary>
        Range_100mV,
        /// <summary>
        /// Device range 200mV.
        /// </summary>
        Range_200mV,
        /// <summary>
        /// Device range 500mV.
        /// </summary>
        Range_500mV,
        /// <summary>
        /// Device range 1V.
        /// </summary>
        Range_1V,
        /// <summary>
        /// Device range 2V.
        /// </summary>
        Range_2V,
        /// <summary>
        /// Device range 5V.
        /// </summary>
        Range_5V,
        /// <summary>
        /// Device range 10V.
        /// </summary>
        Range_10V,
        /// <summary>
        /// Device range 20V.
        /// </summary>
        Range_20V
        //Range_MAX_RANGE
    }
```

* Coupling
```csharp
	/// <summary>
    /// Pico device coupling.
    /// </summary>
    public enum Coupling : uint
    {
        /// <summary>
        /// Device coupling AC.
        /// </summary>
        PS5000A_AC,
        /// <summary>
        /// Device coupling DC.
        /// </summary>
        PS5000A_DC
    }
```

* ReportedTimeUnits
```csharp
	/// <summary>
    /// Pico device capture time units.
    /// </summary>
    public enum ReportedTimeUnits : uint
    {
        /// <summary>
        /// Time unit: FemtoSeconds.
        /// </summary>
        FemtoSeconds,
        /// <summary>
        /// Time unit: PicoSeconds.
        /// </summary>
        PicoSeconds,
        /// <summary>
        /// Time unit: NanoSeconds.
        /// </summary>
        NanoSeconds,
        /// <summary>
        /// Time unit: MicroSeconds.
        /// </summary>
        MicroSeconds,
        /// <summary>
        /// Time unit: MilliSeconds.
        /// </summary>
        MilliSeconds,
        /// <summary>
        /// Time unit: Seconds.
        /// </summary>
        Seconds,
    }
```

### Basic setting

```csharp
	// ChannelA Options
	public Coupling ChannelA_Couping = Coupling.PS5000A_DC;
	public Range ChannleA_Range = Range.Range_2V;
	public float ChannleA_Offsets = 0.0f;

	// ChannelB Options
	public Coupling ChannelB_Couping = Coupling.PS5000A_DC;
	public Range ChannleB_Range = Range.Range_5V;
	public float ChannleB_Offsets = 0.0f;
	
	// ChannelC Options
	// ...
	
	// ChannelD Options
	// ...

	// Raw Capture Options
	public uint DataCaptureSampleInterval = 20;
	public ReportedTimeUnits DataCaptureTimeUnits = ReportedTimeUnits.NanoSeconds;
	public uint DataCaptureSampleCount = 100000;
	public short _DeviceMaximumValue;
```

### init

```csharp
	static PICOUtilityManager managers;
	static PICORawDataListeners ChannelListener;
	managers = new PICOUtilityManager();
	
	// Default
	Flags = managers.OpenPicoDevice();
	_DeviceMaximumValue = managers.DeviceMaximumValue;
	Flags = managers.setChannelBuffers(1024 * 100);
	
	// Data Captures Setting
	managers.setDataCaptureSampleInterval(DataCaptureSampleInterval);
	managers.setDataCaptureTimeUnits(DataCaptureTimeUnits);
	managers.setDataCaptureSampleCount(DataCaptureSampleCount);
	
	// optional Default: 8 bit.
	bool setResult = managers.setDeviceResolution(DeviceResolution.PS5000A_DR_15BIT);
	
	// Data Listener
	ChannelListener = new PICORawDataListeners();
	
	// add Listener
	managers.addChannelRawDataCallBackListener(ChannelListener);
	managers.addCaptureErrorListener(ChannelListener);
	
	// Channel Setting
	
	// ChannelA
	Flags = managers.ChannelA.setChannelEnable();
	// ChannelB
	Flags = managers.ChannelB.setChannelEnable();
	
	// ChannelA
	Flags = managers.ChannelA.setChannelAction(ChannelA_Couping, ChannleA_Range, ChannleA_Offsets);
	// ChannelB
	Flags = managers.ChannelB.setChannelAction(ChannelB_Couping, ChannleB_Range, ChannleB_Offsets);
```

### Start
```csharp
	// streaming start
	Flags = managers.StartRunStreaming(false); // true: Auto Stop, false: not.
```


### Stop
```csharp
	// streaming stop
	Flags = managers.StopRunStreaming();
```

### Close PICO Device
```csharp
	// close pico device
	Flags = managers.ClosePicoDevice();
```

### Data Receive Example
```csharp
class PICORawDataListeners : DataCallBackCaptureInterface
{
	public void OnCaptureError(uint DeviceErrorCode)
	{
		Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss") + "] PICO Error Code: " + DeviceErrorCode);
	}
	
	public void OnCaptureReceive(RawDataCallback RawData)
	{
		string RecvData = string.Empty;
		int Data_Length = RawData.ChannelA_RawData_Max.Length;
		for (int i = 0; i < Data_Length; i++)
		{
			RecvData = "" + adc_to_mv(RawData.ChannelA_RawData_Max[i], RangeInputValue.Range_2V) + "," + adc_to_mv(RawData.ChannelB_RawData_Max[i], RangeInputValue.Range_5V);
			
			// Save RecvData or do something.
			// ....
			// ....
		}
	}
	
	public int adc_to_mv(int rawdata, ushort InputVlaue)
	{
		return (rawdata * InputVlaue) / _DeviceMaximumValue;
	}
}
```