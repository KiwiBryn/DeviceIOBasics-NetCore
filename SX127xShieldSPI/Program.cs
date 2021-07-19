//---------------------------------------------------------------------------------
// Copyright (c) July 2021, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace devMobile.NetCore.SX127xShieldSPI
{
	class Program
	{
		static void Main(string[] args)
		{
			// UputronicsLeds();
			// TransferFullDuplex();
			// ReadWriteChipSelectStandard();
			// ReadWriteChipSelectDiy();
			// TransferFullDuplexBuffers();
			// ReadWriteDiyChipSelectNonStandard();
			// TransferFullDuplexDiySelectNonStandard();
			 TransferFullDuplexBufferBytesRead();
			//TransferFullDuplexBufferBytesWrite();
			//TransferFullDuplexBufferBytesRead();
		}

		static void UputronicsLeds()
		{
			const int RedLedPinNumber = 6;
			const int GreenLedPinNumber = 13;

			GpioController controller = new GpioController(PinNumberingScheme.Logical);

			controller.OpenPin(RedLedPinNumber, PinMode.Output);
			controller.OpenPin(GreenLedPinNumber, PinMode.Output);

			while (true)
			{
				if (controller.Read(RedLedPinNumber) == PinValue.Low)
				{
					controller.Write(RedLedPinNumber, PinValue.High);
					controller.Write(GreenLedPinNumber, PinValue.Low);
				}
				else
				{
					controller.Write(RedLedPinNumber, PinValue.Low);
					controller.Write(GreenLedPinNumber, PinValue.High);
				}

				Thread.Sleep(1000);
			}
		}

		// plain vanilla version didn't work, tried lots of options
		static void TransferFullDuplex()
		{
			byte[] writeBuffer = new byte[1]; // Memory allocation didn't seem to make any difference
			byte[] readBuffer = new byte[1];
			//Span<byte> writeBuffer = stackalloc byte[1];
			//Span<byte> readBuffer = stackalloc byte[1];

			//var settings = new SpiConnectionSettings(0)
			var settings = new SpiConnectionSettings(0, 0)
			//var settings = new SpiConnectionSettings(0, 1)
			{
				ClockFrequency = 5000000,
				//ClockFrequency = 500000, // Frequency didn't seem to make any difference
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						writeBuffer[0] = registerIndex;
						spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
						//Debug.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", writeBuffer[0], readBuffer[0], Convert.ToString(readBuffer[0], 2).PadLeft(8, '0')); // Debug output stopped after roughly 3 times round for loop often debugger would barf as well
						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", writeBuffer[0], readBuffer[0], Convert.ToString(readBuffer[0], 2).PadLeft(8, '0'));

						// Would be nice if SpiDevice has a TransferSequential
						/* 
						writeBuffer[0] = registerIndex;
						spiDevice.TransferSequential(writeBuffer, readBuffer);
						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", writeBuffer[0], readBuffer[0], Convert.ToString(readBuffer[0], 2).PadLeft(8, '0'));
						*/
					}

					Console.WriteLine("");
					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		// Doesn't work at all
		static void ReadWriteChipSelectStandard()
		{
			var settings = new SpiConnectionSettings(0) // Doesn't work
			//	var settings = new SpiConnectionSettings(0, 0) // Doesn't work
			//var settings = new SpiConnectionSettings(0, 1) // Doesn't Work
			{
				ClockFrequency = 5000000,
				ChipSelectLineActiveState = PinValue.Low,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						spiDevice.WriteByte(registerIndex);
						//Thread.Sleep(5); These made no difference
						//Thread.Sleep(10);
						//Thread.Sleep(20);
						//Thread.Sleep(40);
						byte registerValue = spiDevice.ReadByte();

						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, registerValue, Convert.ToString(registerValue, 2).PadLeft(8, '0'));
					}
					Console.WriteLine("");

					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		// Version with DIY ChipSelect did work
		static void ReadWriteChipSelectDiy()
		{
			const int CSPinNumber = 8; // CS0
			//const int CSPinNumber = 7; // CS1

			// DIY CS0 implented with GPIO pin application controls
			GpioController controller = new GpioController(PinNumberingScheme.Logical);

			controller.OpenPin(CSPinNumber, PinMode.Output);
			//controller.Write(CSPinNumber, PinValue.High);

			//var settings = new SpiConnectionSettings(0) // Doesn't work
			var settings = new SpiConnectionSettings(0, 1) // Works, have to point at unused CS1, this could be a problem is other device on CS1
			//var settings = new SpiConnectionSettings(0, 0) // Works, have to point at unused CS0, this could be a problem is other device on CS0
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						controller.Write(CSPinNumber, PinValue.Low);
						spiDevice.WriteByte(registerIndex);
						//Thread.Sleep(2); // This maybe necessary
						byte registerValue = spiDevice.ReadByte();
						controller.Write(CSPinNumber, PinValue.High);

						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, registerValue, Convert.ToString(registerValue, 2).PadLeft(8, '0'));
					}
					Console.WriteLine("");

					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		// Two of three worked
		static void TransferFullDuplexBuffers()
		{
			Span<byte> writeBuffer = stackalloc byte[2];
			Span<byte> readBuffer = stackalloc byte[2];
			//byte[] writeBuffer = new byte[2];
			//byte[] readBuffer = new byte[2];

			var settings = new SpiConnectionSettings(0, 0)
			//var settings = new SpiConnectionSettings(0, 1)
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						// Doesn't work
						writeBuffer[0] = registerIndex;
						spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, readBuffer[0], Convert.ToString(readBuffer[0], 2).PadLeft(8, '0'));

						// Does work
						writeBuffer[0] = registerIndex;
						spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, readBuffer[1], Convert.ToString(readBuffer[1], 2).PadLeft(8, '0'));

						// Does work
						writeBuffer[1] = registerIndex;
						spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, readBuffer[1], Convert.ToString(readBuffer[1], 2).PadLeft(8, '0'));

						Console.WriteLine("");
					}

					Console.WriteLine("");
					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		// Chip select with pin which isn't CS0 or CS1 needs M2M shield
		static void ReadWriteDiyChipSelectNonStandard()
		{
			const int CSPinNumber = 25;

			// DIY CS0 implented with GPIO pin application controls
			GpioController controller = new GpioController(PinNumberingScheme.Logical);

			controller.OpenPin(CSPinNumber, PinMode.Output);
			//controller.Write(CSPinNumber, PinValue.High);

			// Work, this could be a problem is other device on CS0/CS1
			var settings = new SpiConnectionSettings(0)
			//var settings = new SpiConnectionSettings(0, 0) 
			//var settings = new SpiConnectionSettings(0, 1) 
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						controller.Write(CSPinNumber, PinValue.Low);
						spiDevice.WriteByte(registerIndex);
						//Thread.Sleep(2); // This maybe necessary
						byte registerValue = spiDevice.ReadByte();
						controller.Write(CSPinNumber, PinValue.High);

						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, registerValue, Convert.ToString(registerValue, 2).PadLeft(8, '0'));
					}
					Console.WriteLine("");

					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		// Chip select with pin which isn't CS0 or CS1
		static void TransferFullDuplexDiySelectNonStandard()
		{
			Span<byte> writeBuffer = stackalloc byte[2];
			Span<byte> readBuffer = stackalloc byte[2];
			//byte[] writeBuffer = new byte[2];
			//byte[] readBuffer = new byte[2];

			const int CSPinNumber = 25;

			// DIY CS0 implented with GPIO pin application controlled
			GpioController controller = new GpioController(PinNumberingScheme.Logical);
			controller.OpenPin(CSPinNumber, PinMode.Output);

			// Works, have to point at unused CS0/CS1, others could be a problem is another another SPI device is on on CS0/CS1
			var settings = new SpiConnectionSettings(0)
			//var settings = new SpiConnectionSettings(0, 0) 
			//var settings = new SpiConnectionSettings(0, 1) 
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			Thread.Sleep(500);

			while (true)
			{
				try
				{
					for (byte registerIndex = 0; registerIndex <= 0x42; registerIndex++)
					{
						writeBuffer[0] = registerIndex;
						//writeBuffer[1] = registerIndex;

						controller.Write(CSPinNumber, PinValue.Low);
						spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
						controller.Write(CSPinNumber, PinValue.High);

						Console.WriteLine("Register 0x{0:x2} - Value 0X{1:x2} - Bits {2}", registerIndex, readBuffer[1], Convert.ToString(readBuffer[1], 2).PadLeft(8, '0'));
					}
					Console.WriteLine("");

					Thread.Sleep(5000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		static void TransferFullDuplexBufferBytesRead()
		{ 
			const byte length = 3;
			byte[] writeBuffer = new byte[length + 1];
			byte[] readBuffer = new byte[length + 1];

			// Read the frequency which is 3 bytes RegFrMsb 0x6c, RegFrMid 0x80, RegFrLsb 0x00
			writeBuffer[0] = 0x06; //

			// Works, have to point at unused CS0/CS1, others could be a problem is another another SPI device is on on CS0/CS1
			//var settings = new SpiConnectionSettings(0)
			var settings = new SpiConnectionSettings(0, 0) 
			//var settings = new SpiConnectionSettings(0, 1) 
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

			Console.WriteLine($"Register 0x06-0x{readBuffer[1]:x2} 0x07-0x{readBuffer[2]:x2} 0x08-0x{readBuffer[3]:x2}");
		}

		static void TransferFullDuplexBufferBytesWrite()
		{
			const byte length = 3;
			byte[] writeBuffer = new byte[length + 1];
			byte[] readBuffer = new byte[length + 1];

			// Write the frequency which is 3 bytes RegFrMsb 0x6c, RegFrMid 0x80, RegFrLsb or with 0x00 the write mask
			writeBuffer[0] = 0x86 ;

			// Works, have to point at unused CS0/CS1, others could be a problem is another another SPI device is on on CS0/CS1
			//var settings = new SpiConnectionSettings(0)
			var settings = new SpiConnectionSettings(0, 0)
			//var settings = new SpiConnectionSettings(0, 1) 
			{
				ClockFrequency = 5000000,
				Mode = SpiMode.Mode0,   // From SemTech docs pg 80 CPOL=0, CPHA=0
			};

			SpiDevice spiDevice = SpiDevice.Create(settings);

			// Set the frequency to 915MHz
			writeBuffer[1] = 0xE4;
			writeBuffer[2] = 0xC0;
			writeBuffer[3] = 0x00;

			spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
		}
	}
}
