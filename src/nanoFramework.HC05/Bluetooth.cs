﻿using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace nanoFramework.HC05
{
	public class Bluetooth 
	{

		/// <summary>Direct access to Serial Port.</summary>
		public SerialPort serialPort;
		private GpioPin reset; //output
		private GpioPin statusInt; //input interupt
		private Thread readerThread;

		private object _lock = new Object();

		private Client _client;

		private Host _host;

		/// <summary>Gets a value that indicates whether the bluetooth connection is connected.</summary>
		public bool IsConnected
		{
			get
			{
				return this.statusInt.Read()==PinValue.High;
			}
		}

		/// <summary>Sets Bluetooth module to work in Client mode.</summary>
		public Client ClientMode
		{
			get
			{
				lock (_lock)
				{
					if (_host != null) throw new InvalidOperationException("Cannot use both Client and Host modes for Bluetooth module");
					if (_client == null) _client = new Client(this);
					return _client;
				}
			}
		}

		/// <summary>Sets Bluetooth module to work in Host mode.</summary>
		public Host HostMode
		{
			get
			{
				lock (_lock)
				{
					if (_client != null) throw new InvalidOperationException("Cannot use both Client and Host modes for Bluetooth module");
					if (_host == null) _host = new Host(this);
					return _host;
				}
			}
		}

		/// <summary>Possible states of the Bluetooth module</summary>
		public enum BluetoothState
		{

			/// <summary>Module is initializing</summary>
			Initializing = 0,

			/// <summary>Module is ready</summary>
			Ready = 1,

			/// <summary>Module is in pairing mode</summary>
			Inquiring = 2,

			/// <summary>Module is making a connection attempt</summary>
			Connecting = 3,

			/// <summary>Module is connected</summary>
			Connected = 4,

			/// <summary>Module is diconnected</summary>
			Disconnected = 5
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Bluetooth(int resetPin,int statusInteruptPin, string ComPort)
		{

			// This finds the Socket instance from the user-specified socket number. This will generate user-friendly error messages if the socket is invalid. If there is more than one socket on this
			// module, then instead of "null" for the last parameter, put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
			//Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			var gpio = new GpioController();
			this.reset = gpio.OpenPin(resetPin, PinMode.Output); //GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
			this.reset.Write(PinValue.Low);
			this.statusInt = gpio.OpenPin(statusInteruptPin, PinMode.InputPullUp); //GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.serialPort = new SerialPort(ComPort, 38400, Parity.None, 8, StopBits.One); //GTI.SerialFactory.Create(socket, 38400, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);
         	//this.statusInt.Interrupt += GTI.InterruptInputFactory.Create.InterruptEventHandler(statusInt_Interrupt);
			this.serialPort.ReadTimeout = Timeout.Infinite;
			this.serialPort.Open();

			Thread.Sleep(5);
			this.reset.Write(true);

			readerThread = new Thread(new ThreadStart(runReaderThread));
			readerThread.Start();
			Thread.Sleep(500);
		}


        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        /// <param name="baud">The baud rate to communicate with the module with.</param>
        public Bluetooth(int resetPin, int statusInteruptPin, string ComPort, long baud)
		{
			// This finds the Socket instance from the user-specified socket number. This will generate user-friendly error messages if the socket is invalid. If there is more than one socket on this
			// module, then instead of "null" for the last parameter, put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
			//Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			//this.reset = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
			//this.statusInt = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingAndFallingEdge, this);
			//this.serialPort = GTI.SerialFactory.Create(socket, 38400, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);
			var gpio = new GpioController();
			this.reset = gpio.OpenPin(resetPin, PinMode.Output); //GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
			this.reset.Write(PinValue.Low);
			this.statusInt = gpio.OpenPin(statusInteruptPin, PinMode.InputPullUp); //GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.serialPort = new SerialPort(ComPort, 38400, Parity.None, 8, StopBits.One); //GTI.SerialFactory.Create(socket, 38400, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);

			//this.statusInt.Interrupt += GTI.InterruptInputFactory.Create.InterruptEventHandler(statusInt_Interrupt);
			this.serialPort.ReadTimeout = Timeout.Infinite;
			this.serialPort.Open();

			Thread.Sleep(5);
			this.reset.Write(true);

			// Poundy added:
			Thread.Sleep(5);
			this.SetDeviceBaud(baud);
			//this.serialPort.Flush();
			this.serialPort.Close();
			this.serialPort.BaudRate = (int)baud;
			this.serialPort.Open();
			// Poundy

			readerThread = new Thread(new ThreadStart(runReaderThread));
			readerThread.Start();
			Thread.Sleep(500);
		}

		/// <summary>Hard Reset Bluetooth module</summary>
		public void Reset()
		{
			this.reset.Write(false);
			Thread.Sleep(5);
			this.reset.Write(true);
		}

		/// <summary>Sets the device name as seen by other devices</summary>
		/// <param name="name">Name of the device</param>
		public void SetDeviceName(string name)
		{
			this.serialPort.Write("\r\n+STNA=" + name + "\r\n");
		}

		/// <summary>Switch the device to the directed speed</summary>
		/// <param baud="number">Name of the device</param>
		public void SetDeviceBaud(long baud)
		{
			string cmd = "";
			switch (baud)
			{
				case 9600:
					cmd = "9600";
					break;

				case 19200:
					cmd = "19200";
					break;

				case 38400:
					cmd = "38400";
					break;

				case 57600:
					cmd = "57600";
					break;

				case 115200:
					cmd = "115200";
					break;

				case 230400:
					cmd = "230400";
					break;

				case 460800:
					cmd = "460800";
					break;

				default:
					cmd = "";
					break;
			}

			if (cmd != "")
				this.serialPort.Write("\r\n+STBD=" + cmd + "\r\n");
			//todo: check it is working?! Probably should check the return code and do something about it. in the meantime,
			Thread.Sleep(500);
		}

		/// <summary>Sets the PIN code for the Bluetooth module</summary>
		/// <param name="pinCode"></param>
		public void SetPinCode(string pinCode)
		{
			this.serialPort.Write("\r\n+STPIN=" + pinCode + "\r\n");
		}

		/// <summary>Thread that continuously reads incoming messages from the module, parses them and triggers the corresponding events.</summary>
		private void runReaderThread()
		{
			Debug.WriteLine("Reader Thread");
			while (true)
			{
				String response = "";
				while (serialPort.BytesToRead > 0)
				{
					response = response + (char)serialPort.ReadByte();
				}
				if (response.Length > 0)
				{
					Debug.WriteLine(response);

					//Check Bluetooth State Changed
					if (response.IndexOf("+BTSTATE:") > -1)
					{
						string atCommand = "+BTSTATE:";

						//String parsing
						// Return format: +COPS:<mode>[,<format>,<oper>]
						int first = response.IndexOf(atCommand) + atCommand.Length;
						int last = response.IndexOf("\n", first);
						int state = int.Parse(((response.Substring(first, last - first)).Trim()));

						OnBluetoothStateChanged(this, (BluetoothState)state);
					}
					//Check Pin Requested
					if (response.IndexOf("+INPIN") > -1)
					{
						// EDUARDO : Needs testing
						OnPinRequested(this);
					}
					if (response.IndexOf("+RTINQ") > -1)
					{
						//EDUARDO: Needs testing

						string atCommand = "+RTINQ=";
						//String parsing
						int first = response.IndexOf(atCommand) + atCommand.Length;
						int mid = response.IndexOf(";", first);
						int last = response.IndexOf("\r", first);

						// Keep reading until the end of the message
						while (last < 0)
						{
							while (serialPort.BytesToRead > 0)
							{
								response = response + (char)serialPort.ReadByte();
							}
							last = response.IndexOf("\r", first);
						}

						string address = ((response.Substring(first, mid - first)).Trim());

						string name = (response.Substring(mid + 1, last - mid));

						OnDeviceInquired(this, address, name);
						//Debug.WriteLine("Add: " + address + ", Name: " + name );
					}
					else
					{
						OnDataReceived(this, response);
					}
				}
				Thread.Sleep(1);  //poundy changed from thread.sleep(10)
			}
		}

		#region DELEGATES AND EVENTS

		#region Bluetooth State Changed
		private BluetoothStateChangedHandler onBluetoothStateChanged;

		/// <summary>Represents the delegate used for the <see cref="BluetoothStateChanged" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="btState">Current state of the Bluetooth module</param>
		public delegate void BluetoothStateChangedHandler(Bluetooth sender, BluetoothState btState);

		/// <summary>Event raised when the bluetooth module changes its state.</summary>
		public event BluetoothStateChangedHandler BluetoothStateChanged;

		/// <summary>Raises the <see cref="BluetoothStateChanged" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="btState">Current state of the Bluetooth module</param>
		protected virtual void OnBluetoothStateChanged(Bluetooth sender, BluetoothState btState)
		{
			if (onBluetoothStateChanged == null) onBluetoothStateChanged = new BluetoothStateChangedHandler(OnBluetoothStateChanged);
			//if (Program.CheckAndInvoke(BluetoothStateChanged, onBluetoothStateChanged, sender, btState))
			{
				BluetoothStateChanged(sender, btState);
			}
		}

		#endregion Bluetooth State Changed

		#region DataReceived
		private DataReceivedHandler onDataReceived;

		/// <summary>Represents the delegate used for the <see cref="DataReceived" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="data">Data received from the Bluetooth module</param>
		public delegate void DataReceivedHandler(Bluetooth sender, string data);

		/// <summary>Event raised when the bluetooth module changes its state.</summary>
		public event DataReceivedHandler DataReceived;

		/// <summary>Raises the <see cref="DataReceived" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="data">Data string received by the Bluetooth module</param>
		protected virtual void OnDataReceived(Bluetooth sender, string data)
		{
			if (onDataReceived == null) onDataReceived = new DataReceivedHandler(OnDataReceived);
			//if (Program.CheckAndInvoke(DataReceived, onDataReceived, sender, data))
			{
				DataReceived(sender, data);
			}
		}

		#endregion DataReceived

		#region PinRequested
		private PinRequestedHandler onPinRequested;

		/// <summary>Represents the delegate used for the <see cref="PinRequested" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		public delegate void PinRequestedHandler(Bluetooth sender);

		/// <summary>Event raised when the bluetooth module changes its state.</summary>
		public event PinRequestedHandler PinRequested;

		/// <summary>Raises the <see cref="PinRequested" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		protected virtual void OnPinRequested(Bluetooth sender)
		{
			if (onPinRequested == null) onPinRequested = new PinRequestedHandler(OnPinRequested);
			//if (Program.CheckAndInvoke(PinRequested, onPinRequested, sender))
			{
				PinRequested(sender);
			}
		}

		#endregion PinRequested

		#endregion DELEGATES AND EVENTS

		#region DeviceInquired
		private DeviceInquiredHandler onDeviceInquired;

		/// <summary>Represents the delegate used for the <see cref="DeviceInquired" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="macAddress">MAC Address of the inquired device</param>
		/// <param name="name">Name of the inquired device</param>
		public delegate void DeviceInquiredHandler(Bluetooth sender, string macAddress, string name);

		/// <summary>Event raised when the bluetooth module changes its state.</summary>
		public event DeviceInquiredHandler DeviceInquired;

		/// <summary>Raises the <see cref="PinRequested" /> event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="macAddress">MAC Address of the inquired device</param>
		/// <param name="name">Name of the inquired device</param>
		protected virtual void OnDeviceInquired(Bluetooth sender, string macAddress, string name)
		{
			if (onDeviceInquired == null) onDeviceInquired = new DeviceInquiredHandler(OnDeviceInquired);
			//if (Program.CheckAndInvoke(DeviceInquired, onDeviceInquired, sender, macAddress, name))
			{
				DeviceInquired(sender, macAddress, name);
			}
		}

		#endregion DeviceInquired
		/// <summary>Client functionality for the Bluetooth module</summary>
		public class Client
		{
			private Bluetooth bluetooth;

			internal Client(Bluetooth bluetooth)
			{
				Debug.WriteLine("Client Mode");
				this.bluetooth = bluetooth;
				bluetooth.serialPort.Write("\r\n+STWMOD=0\r\n");
			}

			/// <summary>Enters pairing mode</summary>
			public void EnterPairingMode()
			{
				Debug.WriteLine("Enter Pairing Mode");
				bluetooth.serialPort.Write("\r\n+INQ=1\r\n");
			}

			/// <summary>Inputs pin code</summary>
			/// <param name="pinCode">Module's pin code. Default: 0000</param>
			public void InputPinCode(string pinCode)
			{
				Debug.WriteLine("Inputting pin: " + pinCode);
				bluetooth.serialPort.Write("\r\n+RTPIN=" + pinCode + "\r\n");
			}

			/// <summary>Closes current connection. Doesn't work yet.</summary>
			public void Disconnect()
			{
				Debug.WriteLine("Disconnection is not working...");
				//NOT WORKING
				// Documentation states that in order to disconnect, we pull PIO0 HIGH,
				// but this pin is not available in the socket... (see schematics)
			}

			/// <summary>Sends data through the connection.</summary>
			/// <param name="message">String containing the data to be sent</param>
			public void Send(string message)
			{
				Debug.WriteLine("Sending: " + message);
				bluetooth.serialPort.Write(message);
			}

			/// <summary>Sends data through the connection.</summary>
			/// <param name="message">String containing the data to be sent</param>
			public void SendLine(string message)
			{
				Debug.WriteLine("Sending: " + message);
				bluetooth.serialPort.WriteLine(message);
			}
		}

		/// <summary>Implements the host functionality for the Bluetooth module</summary>
		public class Host
		{
			private Bluetooth bluetooth;

			internal Host(Bluetooth bluetooth)
			{
				Debug.WriteLine("Host mode");
				this.bluetooth = bluetooth;
				bluetooth.serialPort.Write("\r\n+STWMOD=1\r\n");
			}

			/// <summary>Starts inquiring for devices</summary>
			public void InquireDevice()
			{
				Debug.WriteLine("Inquiring device");
				bluetooth.serialPort.Write("\r\n+INQ=1\r\n");
			}

			/// <summary>Makes a connection with a device using its MAC address.</summary>
			/// <param name="macAddress">MAC address of the device</param>
			public void Connect(string macAddress)
			{
				Debug.WriteLine("Connecting to: " + macAddress);
				bluetooth.serialPort.Write("\r\n+CONN=" + macAddress + "\r\n");
			}

			/// <summary>Inputs the PIN code.</summary>
			/// <param name="pinCode">PIN code. Default 0000</param>
			public void InputPinCode(string pinCode)
			{
				Debug.WriteLine("Inputting pin: " + pinCode);
				bluetooth.serialPort.Write("\r\n+RTPIN=" + pinCode + "\r\n");
			}

			/// <summary>Closes the current connection. Doesn't work yet.</summary>
			public void Disconnect()
			{
				Debug.WriteLine("Disconnection is not working...");
				//NOT WORKING
				// Documentation states that in order to disconnect, we pull PIO0 HIGH,
				// but this pin is not available in the socket... (see schematics)
			}
		}
	}
}
