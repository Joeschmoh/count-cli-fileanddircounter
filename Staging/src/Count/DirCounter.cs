/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (DirCounter.cs) is part of Count.
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
 * DirCounter.cs - Where the real work gets done, this class recursively 
 *    gathers statistics on all the files and folders under the given root
 *    folder. Data gathered is stored in a DirStats class.
 *
 */

using System;
using System.IO;

namespace Count
{
	class DirCounter
	{
		private DirStats m_DirStats;
		private int m_GetStatsCount;
		private int m_DataLevels;
		private string m_FileFilter;
		private bool m_SkipReparsePoints = false;

		private void Initialize()
		{
			m_DirStats = null;
			m_GetStatsCount = 0;
			m_DataLevels = 1;
			m_FileFilter = "*.*";
		}

		private void SetNewBaseDir(string NewBaseDir)
		{
			m_DirStats = new DirStats();
			m_DirStats.DirName = Util.GetTestedFullPath(NewBaseDir);
		}

		private void AddUpDir(DirectoryInfo CurDirInfo, DirStats CurDirStats, int CurLevel)
		{
			// All the real work gets done here, this function adds up the size of the current 
			// folder and recursively calls itself for any directories found. The "CurLevel" is used 
			// to decide whether stats gathered need to be stored for display or just added to the 
			// previous directory Stats.

			FileInfo[] FileList;

			// Arbitrary check for really large nesting - which is most likely an infinite loop.
			if (m_DataLevels - CurLevel > 1024)
			{
				throw new CountException("Too many nested folders, possible infinite directory loop detected.");
			}

			//populate local settings
			try
			{
				FileList = CurDirInfo.GetFiles(m_FileFilter);
			}
			catch (ArgumentException)
			{
				throw new CountException(string.Format("Illegal characters in file filter \"{0}\".", m_FileFilter));
			}

			foreach (FileInfo FI in FileList)
			{
				CurDirStats.LocalNumFiles++;
				CurDirStats.LocalFileSize += (ulong)FI.Length;
				if ((FI.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
				{
					//hidden file
					CurDirStats.LocalHiddenSize += (ulong)FI.Length;
					CurDirStats.LocalNumHidden++;
				}
				if ((FI.Attributes & FileAttributes.System) == FileAttributes.System)
				{
					//hidden file
					CurDirStats.LocalSystemSize += (ulong)FI.Length;
					CurDirStats.LocalNumSystem++;
				}
			}

			//recursively call directories
			foreach (DirectoryInfo NewDir in CurDirInfo.GetDirectories())
			{
				DirStats NewDirStats = null;

				if ((m_SkipReparsePoints == true) && ((NewDir.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint))
				{
					// Skip - this a junction point.
					continue;
				}

				NewDirStats = new DirStats();

				NewDirStats.DirName = NewDir.FullName;
				try
				{
					AddUpDir(NewDir, NewDirStats, CurLevel - 1);
				}
				catch (UnauthorizedAccessException)
				{
					CurDirStats.TotalErrors++;
				}
				catch (IOException)
				{
					CurDirStats.TotalErrors++;
				}

				CurDirStats.LocalNumDirs++;
				if (CurLevel > 0)
					CurDirStats.AddNestedStats(NewDirStats, true);
				else
					CurDirStats.AddNestedStats(NewDirStats, false);
			}
		}

		public DirCounter()
			: this(".")
		{
		}

		public DirCounter(string BaseDir)
		{
			this.Initialize();
			this.BaseDir = BaseDir;
		}

		public string BaseDir
		{
			get
			{
				return m_DirStats.DirName;
			}
			set
			{
				SetNewBaseDir(value);
			}
		}

		public int DataLevels
		{
			get
			{
				return m_DataLevels;
			}
			set
			{
				if (value >= 0)
					m_DataLevels = value;
				else
					throw new CountException("Invalid number of directory levels.");
			}
		}

		public string FileFilter
		{
			get
			{
				return m_FileFilter;
			}
			set
			{
				if (value == string.Empty)
					throw new ArgumentException("String cannot be empty.", "FileFilter");
				m_FileFilter = value;
			}
		}

		public DirStats Stats
		{
			get
			{
				if (m_GetStatsCount < 1)
					this.Refresh();

				return m_DirStats;
			}
		}

		public bool SkipReparsePoints
		{
			get
			{
				return m_SkipReparsePoints;
			}
			set
			{
				m_SkipReparsePoints = value;
			}
		}

		public void Refresh()
		{
			DirectoryInfo DI = new DirectoryInfo(m_DirStats.DirName);

			m_GetStatsCount++;

			Util.GetDeviceFreeSpace(m_DirStats.DirName,
											out m_DirStats.DeviceFree,
											out m_DirStats.DeviceAvailable,
											out m_DirStats.DeviceSize);

			AddUpDir(DI, m_DirStats, m_DataLevels);
		}
	}
}
