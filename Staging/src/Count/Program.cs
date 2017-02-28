/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (Program.cs) is part of Count.
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
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 *
 * Count - Version 2.2
 * Program.cs - The program entry point, parses the command-line and 
 *    then calls the appropriate classes to do the real work.
 *
 */

using System;

namespace Count
{
	class Program
	{
		// Default Parameters
		static int gLevels = 1;
		static string gRootDir = ".";
		static string gOutputFile = null;
		static string gFileFilter = null;
		static bool gUsePrintableChars = false;
		static bool gSkipReparsePoints = false;
		static bool gShowVersion = false;
		static bool gShowHelp = false;
		static bool gSortList = true;
		static bool gReverseSort = false;
		static int gColumnWidth = -1;
		static DirStatsList.EnumSortType gSortType = DirStatsList.EnumSortType.TotalFileSize;
		
		enum TOutputType
		{
			Normal,
			TotalsOnly,
			Details        // this type isn't implemented.
		};

		static TOutputType gOutputType = TOutputType.Normal;

		static void ParseCommandLine(string[] args)
		{
			bool FoundRootDir = false;
			int TempInt = 1;
			string TempStr = null;

			foreach (string ParamStr in args)
			{
				TempStr = ParamStr.Trim();

				if ((TempStr.StartsWith("/T")) || (TempStr.StartsWith("/t")))
				{
					gOutputType = TOutputType.TotalsOnly;
				}
				else if ((TempStr.StartsWith("/L=")) || (TempStr.StartsWith("/l=")))
				{
					if (int.TryParse(TempStr.Substring(3), out TempInt) != false)
						gLevels = TempInt;
					else
						throw new CountException(string.Format("Number of levels is invalid - \"{0}\".", TempStr));
				}
				else if ((TempStr.StartsWith("/N")) || (TempStr.StartsWith("/n")))
				{
					gUsePrintableChars = true;
				}
				else if ((TempStr.StartsWith("/J")) || (TempStr.StartsWith("/j")))
				{
					gSkipReparsePoints = true;
				}
				else if ((TempStr.StartsWith("/O=")) || (TempStr.StartsWith("/o=")))
				{
					gOutputFile = TempStr.Substring(3);
					if (gOutputFile == string.Empty)
						throw new CountException("No output file specified.");
				}
				else if ((TempStr.StartsWith("/OW=")) || (TempStr.StartsWith("/ow=")))
				{
					if (int.TryParse(TempStr.Substring(4), out TempInt) != false)
						gColumnWidth = TempInt;
					else
						throw new CountException(string.Format("Column width is invalid - \"{0}\".", TempStr));

					if ((gColumnWidth < 80) || (gColumnWidth > 250))
						throw new CountException("Column width value must 80-250 (inclusive).");
				}
				else if ((TempStr.StartsWith("/F=")) || (TempStr.StartsWith("/f=")))
				{
					gFileFilter = TempStr.Substring(3);
					if (gFileFilter == string.Empty)
						throw new CountException("No file filter specified.");
				}
				else if ((TempStr.StartsWith("/S=")) || (TempStr.StartsWith("/s=")))
				{
					TempStr = TempStr.ToUpper();
					if ((TempStr.EndsWith("R")) || (TempStr.EndsWith("r")))
					{
						gReverseSort = true;
						TempStr = TempStr.Remove(4);
					}
					else
					{
						gReverseSort = false;
					}
					switch (TempStr.Substring(3))
					{
						case "N":
							gSortList = true;
							gSortType = DirStatsList.EnumSortType.DirName;
							break;

						case "S":
							gSortList = true;
							gSortType = DirStatsList.EnumSortType.TotalFileSize;
							break;

						case "X":
							gSortList = false;
							break;

						default:
							throw new CountException("Sort type must be 'N', 'S', or 'X', with an optional 'R' appended.");
					}
				}
				else if ((TempStr.StartsWith("/V")) || (TempStr.StartsWith("/v")))
				{
					gShowVersion = true;
				}
				else if (TempStr.StartsWith("/?"))
				{
					gShowHelp = true;
				}
				else if (TempStr.StartsWith("/"))
				{
					throw new CountException(string.Format("Invalid command-line switch \"{0}\", run with /? for help.", ParamStr));
				}
				else
				{
					if (FoundRootDir)
						throw new CountException("Found multiple root directories listed.");

					FoundRootDir = true;
					gRootDir = TempStr;
				}
			}
		}

		static void Main(string[] args)
		{
			DirCounter MyCounter = null;
			DirOutput MyOutput = null;

			// Master "try" clause to catch any Count specific errors.
			try
			{
				// Parse Command-Line
				ParseCommandLine(args);

				// Prepare output object
				switch (gOutputType)
				{
					case TOutputType.TotalsOnly:
						MyOutput = new DirOutput();
						break;

					case TOutputType.Details:
						MyOutput = new DirOutputDetails();
						break;

					case TOutputType.Normal:
					default:
						MyOutput = new DirOutputLineItems();
						break;
				}

				if (gOutputFile != null)
				{
					try
					{
						MyOutput.OutWriter = new System.IO.StreamWriter(gOutputFile);
					}
					catch (System.IO.IOException)
					{
						throw new CountException(string.Format("Unable to create output file \"{0}\".", gOutputFile));
					}
					catch (ArgumentException)
					{
						throw new CountException(string.Format("Output file has illegal characters \"{0}\".", gOutputFile));
					}
				}

				MyOutput.UsePrintableChars = gUsePrintableChars;
				if (gColumnWidth > 0)
				{
					MyOutput.DisplayColumnSize = gColumnWidth;
				}

				// Initial output - posting a banner makes it feel like the program is
				// working while it counts up folders.
				if (gShowVersion)
				{
					// Special case #1 - show version and exit, no banner.
					MyOutput.DoVersionInfo();
					return;
				}

				// This outputs a banner.
				MyOutput.DoHeader();

				if (gShowHelp)
				{
					// Special case #2 - show help and exit 
					MyOutput.DoHelp();
					return;
				}

				// Prepare the counter object
				MyCounter = new DirCounter(gRootDir);
				MyCounter.DataLevels = gLevels;
				MyCounter.SkipReparsePoints = gSkipReparsePoints;
				if (gFileFilter != null)
				{
					MyCounter.FileFilter = gFileFilter;
				}

				// Count
				MyCounter.Refresh();

				// Output
				if (gSortList == true)
				{
					MyCounter.Stats.SortChildStats(gSortType); //, gReverseSort); // you can either sort in reverse, or print list in reverse below.
				}

				MyOutput.PrintItemsInOrder = !gReverseSort;
				MyOutput.Stats = MyCounter.Stats;
				MyOutput.DoOutput();
			}
			catch (CountException Ex)
			{
				Console.Error.Write("Error: ");
				Console.Error.WriteLine(Ex.Message);
			}
		}
	}
}
