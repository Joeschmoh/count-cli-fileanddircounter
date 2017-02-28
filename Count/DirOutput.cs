/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (DirOutput.cs) is part of Count.
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
 * DirOutput.cs - A set of classes to display the output of a "DirStats" 
 *    class. Base class only does the final statistics, super classes are
 *    used to do customized output. Command-line options determine which 
 *    class is instantiated and therefore what output is shown.
 *
 */

using System;
using System.IO;
using System.Reflection;

namespace Count
{
	class DirOutput  // base output class - this one only does totals.
	{
		private bool m_DisplayColumnSizeIsDefault = true;

		private void ComputeDefaultColumnSize()
		{
			if (m_DisplayColumnSizeIsDefault == true)
			{
				if (m_Out == Console.Out)
					m_DisplayColumnSize = Console.WindowWidth < 80 ? 80 : Console.WindowWidth;
				else
					m_DisplayColumnSize = 80;
			}
		}

		private string MakeBannerLine(string Text, string BannerChar, int BannerWidth)
		{
			int i;
			int CenterSpace = (BannerWidth - BannerChar.Length * 2 - 2) / 2 - Text.Length / 2;
			string Result = String.Empty;

			// Initial "*<space><centering spaces>"
			Result = BannerChar + " ";
			for (i = 0; i < CenterSpace; i++)
				Result += " ";

			// Put in the text
			Result += Text;

			// Space out until end of Banner
			CenterSpace = Result.Length;
			for (i = BannerWidth - BannerChar.Length; i > CenterSpace; i--)
				Result += " ";

			// Now last Banner Characters
			Result += BannerChar;

			return Result;
		}

		// Protected here so a super class can see these items.
		protected DirStats m_Stats = null;
		protected int m_Levels = 100000;
		protected TextWriter m_Out = null;
		protected bool m_UsePrintable = false;
		protected int m_DisplayColumnSize = 80;

		protected virtual void DoTotals()
		{
			ulong TotalSize = m_Stats.LocalFileSize + m_Stats.NestedFileSize;
			long SlackSpace = (long)(m_Stats.DeviceSize - m_Stats.DeviceFree - TotalSize);
			string TempString;

			m_Out.WriteLine();
			m_Out.WriteLine("{0}:", m_Stats.DirName);
			m_Out.WriteLine("   Total Space(all folders): {0,8} ({1:N0} bytes)",
									Util.ULongToHumanReadable(TotalSize),
									TotalSize);
			m_Out.WriteLine("   Total Space(root folder): {0,8} ({1:N0} bytes)",
									Util.ULongToHumanReadable(m_Stats.LocalFileSize),
									m_Stats.LocalFileSize);
			m_Out.WriteLine("   Hidden Files:             {0,8} in {1:N0} file(s)",
									Util.ULongToHumanReadable(m_Stats.LocalHiddenSize + m_Stats.NestedHiddenSize),
									m_Stats.LocalNumHidden + m_Stats.NestedNumHidden);
			m_Out.WriteLine("   System Files:             {0,8} in {1:N0} file(s)",
									Util.ULongToHumanReadable(m_Stats.LocalSystemSize + m_Stats.NestedSystemSize),
									m_Stats.LocalNumSystem + m_Stats.NestedNumSystem);
			m_Out.WriteLine("   Percentage in use:        {0,8:#0.00%} in {1:N0} file(s) and {2:N0} dir(s)",
									(double)TotalSize / m_Stats.DeviceSize,
									m_Stats.LocalNumFiles + m_Stats.NestedNumFiles,
									m_Stats.LocalNumDirs + m_Stats.NestedNumDirs);
			m_Out.WriteLine("   Device Size:              {0,8} ({1:N0} bytes)",
									Util.ULongToHumanReadable(m_Stats.DeviceSize),
									m_Stats.DeviceSize);
			m_Out.WriteLine("   Slack Space:              {0,8} ({1:#0.00%})",
									Util.LongToHumanReadable(SlackSpace),
									(double)SlackSpace / m_Stats.DeviceSize);

			TempString = String.Format("   Total Free:               {0,8} ({1:#0.00%}, {2:N0} bytes, {3} available)",
									Util.ULongToHumanReadable(m_Stats.DeviceFree),
									(double)m_Stats.DeviceFree / m_Stats.DeviceSize,
									m_Stats.DeviceFree,
									Util.ULongToHumanReadable(m_Stats.DeviceAvailable));
			if (TempString.Length <= m_DisplayColumnSize)
			{
				m_Out.WriteLine("   Total Free:               {0,8} ({1:#0.00%}, {2:N0} bytes, {3} available)",
										Util.ULongToHumanReadable(m_Stats.DeviceFree),
										(double)m_Stats.DeviceFree / m_Stats.DeviceSize,
										m_Stats.DeviceFree,
										Util.ULongToHumanReadable(m_Stats.DeviceAvailable));
			}
			else
			{
				m_Out.WriteLine("   Total Free:               {0,8} ({1:#0.00%}, {2:N0} bytes,",
										Util.ULongToHumanReadable(m_Stats.DeviceFree),
										(double)m_Stats.DeviceFree / m_Stats.DeviceSize,
										m_Stats.DeviceFree);
				m_Out.WriteLine("                                       {0} available)",
										Util.ULongToHumanReadable(m_Stats.DeviceAvailable));
			}

			if (m_Stats.TotalErrors > 0)
			{
				m_Out.WriteLine();
				if (m_Stats.TotalErrors == 1)
				{
					m_Out.WriteLine("** There was 1 error detected which may skew the results.");
				}
				else
				{
					m_Out.WriteLine("** There were {0:N0} errors detected which may skew the results.",
										  m_Stats.TotalErrors);
				}
			}
		}

		public DirOutput()
		{
			PrintItemsInOrder = true;
			OutWriter = Console.Out;
		}

		public int DisplayColumnSize
		{
			get
			{
				return m_DisplayColumnSize;
			}
			set
			{
				m_DisplayColumnSizeIsDefault = false;

				if (value < 80)
					m_DisplayColumnSize = 80;
				else
					m_DisplayColumnSize = value;
			}
		}

		public DirStats Stats
		{
			get
			{
				return m_Stats;
			}
			set
			{
				m_Stats = value;
			}
		}

		public int Levels
		{
			get
			{
				return m_Levels;
			}
			set
			{
				if (m_Levels < 0)
					m_Levels = -1;
				else
					m_Levels = value;
			}
		}

		public bool UsePrintableChars
		{
			get
			{
				return m_UsePrintable;
			}
			set
			{
				m_UsePrintable = value;
			}
		}

		public bool PrintItemsInOrder
		{
			get;
			set;
		}

		public TextWriter OutWriter
		{
			get
			{
				return m_Out;
			}
			set
			{
				if (value != null)
					m_Out = value;
				else
					throw new CountException("Bad output device set.");

				ComputeDefaultColumnSize();
			}
		}

		public virtual void DoVersionInfo()
		{
			AssemblyInfo MyExeInfo = new AssemblyInfo();

			m_Out.WriteLine(MyExeInfo.Name + " - " + MyExeInfo.Description);
			m_Out.WriteLine("Version: " + MyExeInfo.Version);
			m_Out.WriteLine(MyExeInfo.Copyright);
			m_Out.WriteLine();
			
			m_Out.WriteLine(MyExeInfo.Trademark);
			if (MyExeInfo.Trademark.Contains("GPL"))
			{
				m_Out.WriteLine("This is free software: you are free to change and redistribute it.");
				m_Out.WriteLine("There is NO WARRANTY, to the extent permitted by law.");
			}
		}

		public virtual void DoHeader()
		{
			// This has to be a great example of how to make a function larger
			// than it has to be. Simple would be to just dump the title, version
			// and copyright. But apparently I had to make it fancy.

			AssemblyInfo MyExeInfo = new AssemblyInfo();
			string CenterSpaceString = String.Empty;

			// used to create banner
			int i = 0;
			int BannerWidth = 0;
			int CenterSpace = 0;
			int LongestLineSize = 0;
			int BannerSmallestLineSize = m_DisplayColumnSize;

			//Banner strings
			string BannerChar = "**";
			string BannerLine = String.Empty; // will be made later.
			string TitleLine = MyExeInfo.Name + " - " + MyExeInfo.Description;
			string VersionLine = "Version " + MyExeInfo.Version;
			string CopyrightLine = MyExeInfo.Copyright;

			// Which banner string is longer?
			LongestLineSize = TitleLine.Length;
			if (LongestLineSize < VersionLine.Length)
			{
				LongestLineSize = VersionLine.Length;
			}
			if (LongestLineSize < CopyrightLine.Length)
			{
				LongestLineSize = CopyrightLine.Length;
			}
			BannerWidth = LongestLineSize + BannerChar.Length * 2 + 2; // BannerChar on each side, plus a space.

			// Is the banner string langer than our display width?
			if (BannerWidth > BannerSmallestLineSize)
			{
				// too big, just dump the info and don't try to make it pretty.
				m_Out.WriteLine(TitleLine);
				m_Out.WriteLine(VersionLine);
				m_Out.WriteLine(CopyrightLine);
				return;
			}

			// Make the Banner line (all stars)
			for (i = 0; i < BannerWidth; i += BannerChar.Length)
				BannerLine += BannerChar;
			BannerLine = BannerLine.Substring(0, BannerWidth);

			// Put BannerChar's in the TitleLine & VersionLine
			TitleLine = MakeBannerLine(TitleLine, BannerChar, BannerWidth);
			VersionLine = MakeBannerLine(VersionLine, BannerChar, BannerWidth);
			CopyrightLine = MakeBannerLine(CopyrightLine, BannerChar, BannerWidth);

			// Now create spaces to center the whole banner.
			CenterSpace = (m_DisplayColumnSize / 2) - (BannerWidth / 2);
			if (CenterSpace < 0)
				CenterSpace = 0;
			for (i = 0; i < CenterSpace; i++)
				CenterSpaceString += " ";

			m_Out.WriteLine(CenterSpaceString + BannerLine);
			m_Out.WriteLine(CenterSpaceString + TitleLine); 
			m_Out.WriteLine(CenterSpaceString + VersionLine);
			m_Out.WriteLine(CenterSpaceString + CopyrightLine);
			m_Out.WriteLine(CenterSpaceString + BannerLine);
		}

		public virtual void DoHelp()
		{
			m_Out.WriteLine();
			m_Out.WriteLine("Syntax: COUNT [RootDir] [/T] [/L=1] [/N] [/F=x] [/S=x] [/J] [/O=x]");
			m_Out.WriteLine();
			m_Out.WriteLine("Where:");
			m_Out.WriteLine("  /T    = Show summary totals only.");
			m_Out.WriteLine("  /F=x  = Count files filtered by \"x\" (default is *.*).");
			m_Out.WriteLine("  /L=x  = Display \"x\" directory levels (default is 1).");
			m_Out.WriteLine("  /S=x  = Sort list by \"N\" - Name, \"S\" - Size, or \"X\" - don't sort,");
			m_Out.WriteLine("          add \"R\" to reverse sort (default sorts low to high by size).");
			m_Out.WriteLine("  /N    = Display using ASCII characters only.");
			m_Out.WriteLine("  /J    = Skip junction/re-parse points (default is follow).");
			m_Out.WriteLine("  /O=x  = Send output to file \"x\".");
			m_Out.WriteLine("  /OW=x = Set output line width to \"x\".");
			m_Out.WriteLine();
			m_Out.WriteLine("  /V    = Display version information.");
			m_Out.WriteLine("  /?    = Display this screen.");
			m_Out.WriteLine();
			m_Out.WriteLine("Examples:");
			m_Out.WriteLine("   count /t c:\\Windows - show totals only.");
			m_Out.WriteLine("   count /l=3 /s=sr c:\\ - show 3 directory levels reverse sorted by size.");
			m_Out.WriteLine("   count /l=2 /f=*.jpg /j /s=n c:\\Pictures");
			m_Out.WriteLine("      - count jpeg's only, 2 directory levels, skip junctions, sort by name.");
		}

		public virtual void DoOutput()
		{
			if (m_Stats == null)
				throw new CountException("Bad Programming.");

			DoTotals();

			m_Out.Flush();
		}
	}

	class DirOutputLineItems : DirOutput
	{
		private string[,] m_Lines = 
		{ 
			{ "└──", "├──", "│  " },  // directory lines using extended chars.
			{ "`--", "+--", "|  " }   // directory lines using ASCII (7-bit) chars.
		};
		private int m_LI = 0;

		private char GetSpacerCharacter(int Level)
		{
			Level = Level % 5;
			switch (Level)
			{
				case 0:
					return '=';

				case 1:
					return '-';

				case 2:
					return '+';

				case 3:
					return '*';

				case 4:
					return '.';
			}

			return '.';
		}

		private void PrintStatLine(DirStats CurStats, int Level, string Prefix, bool LastChild)
		{
			// Real work done for printing line items - this item will recursively call itself as
			// it finds subfolders. Levels, Prefix, and LastChild are used to nest the output, with
			// LastChild a hint that it's the last folder at this level allowing the line characters
			// to be closed. (Best way to understand this is to study the output of a bunch of nested
			// folders.)

			int i;
			int TLen;
			string DirName;
			int ColumnWidth = m_DisplayColumnSize - 8;
			char SpacerCharacter = GetSpacerCharacter(Level);

			// Check inputs
			if (Level * 3 > ColumnWidth - 1)
				return;

			// Space to name
			if (Level > 0)
			{
				i = Prefix.Length + 3;
				m_Out.Write(Prefix);
				if (LastChild)
					m_Out.Write(m_Lines[m_LI, 0]);
				else
					m_Out.Write(m_Lines[m_LI, 1]);
			}
			else
			{
				i = 0;
			}


			// Print name
			if (CurStats.TotalErrors > 0)
			{
				DirName = "*";
			}
			else
			{
				DirName = "";
			}

			if (Level == 0)
			{
				DirName += CurStats.DirName;
			}
			else
			{
				DirName += System.IO.Path.GetFileName(CurStats.DirName);
			}

			// Truncate name if it's too long
			if (DirName.Length + i > ColumnWidth - 1)
			{
				TLen = ColumnWidth - i - 2;
				if (TLen < 0)
				{
					TLen = 0;
				}
				DirName = DirName.Substring(0, TLen);
			}

			i += DirName.Length;
			m_Out.Write(DirName);

			// Space to Size
			for (; i < ColumnWidth; i++)
			{
				if (i % 2 == 0)
				{
					m_Out.Write(SpacerCharacter);
				}
				else
				{
					m_Out.Write(" ");
				}
			}

			// Print Size
			m_Out.WriteLine("{0,7}", Util.ULongToHumanReadable(CurStats.LocalFileSize + CurStats.NestedFileSize));

			// Child Items
			if (Level < m_Levels) // Print children if it will fit
			{
				i = 0;
				if (LastChild)
				{
					Prefix += "   ";
				}
				else
				{
					Prefix += m_Lines[m_LI, 2];
				}
				
				CurStats.GetChildStats().EnumerateListHeadToTail = this.PrintItemsInOrder;
				foreach (DirStats NewStat in CurStats.GetChildStats())
				{
					i++;
					if (i < CurStats.GetChildStats().Count)
					{
						PrintStatLine(NewStat, Level + 1, Prefix, false);
					}
					else
					{
						PrintStatLine(NewStat, Level + 1, Prefix, true);
					}
				}
			}
		}

		protected virtual void DoLineItems()
		{
			m_Out.WriteLine();
			PrintStatLine(m_Stats, 0, "", true);
		}

		public override void DoOutput()
		{
			// Overrides base class, prints folders as line items (this class),
			//  then outputs totals (base class).

			if (m_Stats == null)
				throw new CountException("Bad Programming.");

			if (m_UsePrintable)
				m_LI = 1;
			else
				m_LI = 0;

			DoLineItems();

			DoTotals();

			// this makes sure all data is available for viewing if to a file. 
			m_Out.Flush();
		}
	}

	class DirOutputDetails : DirOutput
	{
		// not implemented, will just do totals like the base class.
	}
}
