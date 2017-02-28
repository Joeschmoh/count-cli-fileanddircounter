/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (Util.cs) is part of Count.
 *
 *  Count is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Count is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Count.  If not, see <http://www.gnu.org/licenses/>.
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *
 * Count - Version 2.2
 * Util.cs - A class with static members used to hold helpful functions.
 *
 */

using System;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Count
{
	class Util
	{
		public static string GetTestedFullPath(string DirName)
		{
			// This function will - 
			//   1. Test if the directory exists.
			//   2. Catch the most likely exceptions so they can be 
			//      converted into a "Count" exception.
			//   3. Return the full path to the directory (if it exists).

			DirectoryInfo DI;

			try
			{
				DI = new DirectoryInfo(DirName);
				if (DI.Exists != true)
				{
					throw new CountException(System.String.Format("Directory not found: \"{0}\"", DirName));
				}
			}
			catch (ArgumentException)
			{
				throw new CountException(System.String.Format("Invalid directory name specified: \"{0}\"", DirName));
			}
			catch (NotSupportedException)
			{
				throw new CountException(System.String.Format("Invalid directory name specified: \"{0}\"", DirName));
			}
			catch (PathTooLongException)
			{
				throw new CountException(System.String.Format("Directory name is too long: \"{0}\"", DirName));
			}
			catch (SecurityException)
			{
				throw new CountException(System.String.Format("Invalid permission on directory: \"{0}\"", DirName));
			}

			return DI.FullName;
		}

		private static string ExponentToSuffix(long Exponent)
		{
			// These are binary prefixes defined by the IEC, see the
			// article - <http://en.wikipedia.org/wiki/Binary_prefix> for 
			// more info.

			switch (Exponent)
			{
				case 0:
					return "B  ";
				case 3:
					return "KiB";
				case 6:
					return "MiB";
				case 9:
					return "GiB";
				case 12:
					return "TiB";
				case 15:
					return "PiB";
				default:
					return "?iB";
			}
		}

		public static string ULongToHumanReadable(ulong Value)
		{
			// In this case we break things down by 1,024 units (2^10) to use
			// standard IEC binary prefixes, NOT decimal equivalents.

			double Mantissa = Value;
			long Exponent = 0;

			while (Mantissa > 999.9)
			{
				Mantissa = Mantissa / 1024;
				Exponent += 3;
			}

			if (Mantissa < 10)
			{
				return string.Format("{0:0.0} {1}", Mantissa, ExponentToSuffix(Exponent));
			}
			else
			{
				return string.Format("{0:#0} {1}", Mantissa, ExponentToSuffix(Exponent));
			}
		}

		public static string LongToHumanReadable(long Value)
		{
			bool Negative = (Value < 0);
			string result;

			if (Negative)
				Value *= -1;

			result = ULongToHumanReadable((ulong)Value);

			if (Negative)
				result = "-" + result;

			return result;
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
														  ref ulong lpFreeBytesAvailable,
														  ref ulong lpTotalNumberOfBytes,
														  ref ulong lpTotalNumberOfFreeBytes);

		public static void GetDeviceFreeSpace(string PathName,
															 out ulong FreeAvailable,
															 out ulong FreeTotal,
															 out ulong DeviceSize)
		{
			// this function is meant to abstract the OS specific call from the application, 
			// currenty this only handles Windows.

			ulong FBA = 0, TB = 0, TFB = 0;

			if (!PathName.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
				PathName += System.IO.Path.DirectorySeparatorChar;

			if (GetDiskFreeSpaceEx(PathName, ref FBA, ref TB, ref TFB) == false)
			{
				throw new CountException("Unable to retrieve device free space info.");
			}

			FreeAvailable = FBA;
			FreeTotal = TFB;
			DeviceSize = TB;
		}
	}

	class AssemblyInfo
	{
		/* Author's comment - either I'm an idiot or this is overly complex. All I want 
		 * is some basic information found in the AssemblyInfo.cs file. Why all the code
		 * to do this? Is there an easier way? 
		 */

		private Assembly m_asm = null;

		// attributes here
		readonly string m_name;
		readonly string m_version;
		readonly string m_copyright;
		readonly string m_company;
		readonly string m_description;
		readonly string m_trademark;

		private string GetAssemblyName()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyTitleAttribute)attrs[0]).Title;
			}

			return result;
		}

		private string GetAssemblyVersion()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyFileVersionAttribute)attrs[0]).Version;
			}

			return result;
		}

		private string GetAssemblyCopyright()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyCopyrightAttribute)attrs[0]).Copyright;
			}

			return result;
		}

		private string GetAssemblyCompany()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyCompanyAttribute)attrs[0]).Company;
			}

			return result;
		}

		private string GetAssemblyDescription()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyDescriptionAttribute)attrs[0]).Description;
			}

			return result;
		}

		private string GetAssemblyTrademark()
		{
			object[] attrs = null;
			string result = "";

			attrs = m_asm.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
			if ((attrs != null) && (attrs.Length > 0))
			{
				result = ((AssemblyTrademarkAttribute)attrs[0]).Trademark;
			}

			return result;
		}

		public AssemblyInfo()
		{
			m_asm = this.GetType().Assembly;

			m_name = GetAssemblyName();
			m_version = GetAssemblyVersion();
			m_copyright = GetAssemblyCopyright();
			m_company = GetAssemblyCompany();
			m_description = GetAssemblyDescription();
			m_trademark = GetAssemblyTrademark();
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public string Version
		{
			get
			{
				return m_version;
			}
		}

		public string Copyright
		{
			get
			{
				return m_copyright;
			}
		}

		public string Company
		{
			get
			{
				return m_company;
			}
		}

		public string Description
		{
			get
			{
				return m_description;
			}
		}

		public string Trademark
		{
			get
			{
				return m_trademark;
			}
		}
	}
}
