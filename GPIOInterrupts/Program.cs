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
using System.Diagnostics;
using System.Threading;

namespace devMobile.NetCore.GPIOInterrupts
{
	class Program
	{
		private const int ButtonPinNumber = 5;
		private const int LedPinNumber = 16;
		private static GpioController gpiocontroller;

		static void Main(string[] args)
		{
			try
			{
				gpiocontroller = new GpioController(PinNumberingScheme.Logical);

				gpiocontroller.OpenPin(ButtonPinNumber, PinMode.InputPullDown);
				gpiocontroller.OpenPin(LedPinNumber, PinMode.Output);

				gpiocontroller.RegisterCallbackForPinValueChangedEvent(ButtonPinNumber, PinEventTypes.Rising, PinChangeEventHandler);

				Debug.WriteLine($"Main thread:{Thread.CurrentThread.ManagedThreadId}");

				while (true)
				{
					Debug.WriteLine($"Doing stuff");
					Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private static void PinChangeEventHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
		{
			Debug.Write($"Interrupt Thread:{Thread.CurrentThread.ManagedThreadId}");

			if (pinValueChangedEventArgs.ChangeType == PinEventTypes.Rising)
			{
				if (gpiocontroller.Read(LedPinNumber) == PinValue.Low)
				{
					gpiocontroller.Write(LedPinNumber, PinValue.High);
				}
				else
				{
					gpiocontroller.Write(LedPinNumber, PinValue.Low);
				}
			}
		}
	}
}
