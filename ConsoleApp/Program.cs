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
using System.Diagnostics;
using System.Threading;

namespace devMobile.NetCore.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			//Debugger.Break();

			while (true)
			{
				Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Hello World!");

				Thread.Sleep(1000);
			}
		}
	}
}
