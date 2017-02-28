/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (DirStats.cs) is part of Count.
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
 * DirStats.cs - A data class - DirCounter gathers statistics and stores it 
 *    in this class. Uses a linked list to store data for multiple folders, 
 *    simple parent / child relationship maintains the hierarchy.
 *
 */

using System;

namespace Count
{
	class DirStatsList : DOSLinkedList
	{
		// This is a container class for "DirStats" - A "DirStats" is statistics on a single folder, 
		// and the list is used to store multiple folders in a container. The list is used to store child
		// folders stats for a parent retaining the folder hierarchy found while scanning directories.

		private int m_ReverseSortModifier = 1;

		protected override int CompareNodes(DOSLinkedListNode Node1, DOSLinkedListNode Node2)
		{
			int Result = 0;
			DirStats Item1;
			DirStats Item2;
			ulong Total1, Total2 = 0;

			if ((Node1 == null) || (Node2 == null))
				return 0;

			Item1 = (DirStats)Node1.Data;
			Item2 = (DirStats)Node2.Data;

			switch (SortType)
			{
				case DirStatsList.EnumSortType.DirName:
					Result = Item1.DirName.CompareTo(Item2.DirName);
					break;

				case DirStatsList.EnumSortType.TotalFileSize:
				default:
					Total1 = (Item1.NestedFileSize + Item1.LocalFileSize);
					Total2 = (Item2.NestedFileSize + Item2.LocalFileSize);
					
					// These are ulongs, I could type cast to int and just do math, but I
					// I think my method is more determinant and avoids weird cases
					// where integers (even 64-bit) might wrap around.

					if (Total1 == Total2)
						Result = 0;
					else
					{
						if (Total1 > Total2)
							Result = 1;
						else
							Result = -1;
					}
					break;
			}

			return Result * m_ReverseSortModifier;
		}

		public enum EnumSortType
		{
			DirName,
			TotalFileSize
		};

		public EnumSortType SortType
		{
			get;
			set;
		}

		public bool ReverseSort
		{
			get
			{
				if (m_ReverseSortModifier > 0)
					return false;
				else
					return true;
			}
			set
			{
				if (value == true)
					m_ReverseSortModifier = -1;
				else
					m_ReverseSortModifier = 1;
			}
		}
	}

	class DirStats
	{
		private DirStatsList m_ChildList = new DirStatsList();

		private void Initialize()
		{
			DirName = String.Empty;
			this.Reset();
		}

		public string DirName;
		public ulong NestedFileSize;
		public ulong NestedNumFiles;
		public ulong NestedNumHidden;
		public ulong NestedNumSystem;
		public ulong NestedNumDirs;
		public ulong NestedHiddenSize;
		public ulong NestedSystemSize;
		public ulong NestedDeepestLevel;

		public ulong LocalFileSize;
		public ulong LocalNumFiles;
		public ulong LocalNumHidden;
		public ulong LocalNumSystem;
		public ulong LocalNumDirs;
		public ulong LocalHiddenSize;
		public ulong LocalSystemSize;

		public ulong DeviceSize;
		public ulong DeviceFree;
		public ulong DeviceAvailable;

		public ulong TotalErrors;

		public DirStats()
		{
			this.Initialize();
		}

		public void Reset()
		{
			NestedFileSize = 0;
			NestedNumFiles = 0;
			NestedNumHidden = 0;
			NestedNumSystem = 0;
			NestedNumDirs = 0;
			NestedHiddenSize = 0;
			NestedSystemSize = 0;
			NestedDeepestLevel = 0;
			LocalFileSize = 0;
			LocalNumFiles = 0;
			LocalNumHidden = 0;
			LocalNumSystem = 0;
			LocalNumDirs = 0;
			LocalHiddenSize = 0;
			LocalSystemSize = 0;
			DeviceSize = 0;
			DeviceFree = 0;
			DeviceAvailable = 0;
			TotalErrors = 0;

			m_ChildList.Clear();
		}

		public DirStatsList GetChildStats()
		{
			return m_ChildList;
		}

		public void AddNestedStats(DirStats NewStats, bool StoreNested)
		{
			ulong DeepestLevel = NewStats.NestedDeepestLevel + 1;

			this.NestedFileSize += NewStats.NestedFileSize + NewStats.LocalFileSize;
			this.NestedHiddenSize += NewStats.NestedHiddenSize + NewStats.LocalHiddenSize;
			this.NestedNumDirs += NewStats.NestedNumDirs + NewStats.LocalNumDirs;
			this.NestedNumFiles += NewStats.NestedNumFiles + NewStats.LocalNumFiles;
			this.NestedNumHidden += NewStats.NestedNumHidden + NewStats.LocalNumHidden;
			this.NestedNumSystem += NewStats.NestedNumSystem + NewStats.LocalNumSystem;
			this.NestedSystemSize += NewStats.NestedSystemSize + NewStats.LocalSystemSize;
			this.TotalErrors += NewStats.TotalErrors;

			if (this.NestedDeepestLevel < DeepestLevel)
				this.NestedDeepestLevel = DeepestLevel;

			if (StoreNested == true)
			{
				m_ChildList.AddLast(NewStats);
			}
		}

		public void SortChildStats(DirStatsList.EnumSortType SortType, bool ReverseSort = false)
		{
			// Sort our own childen.
			m_ChildList.SortType = SortType;
			m_ChildList.ReverseSort = ReverseSort;
			m_ChildList.SortList();

			// Now tell our children to sort their children.
			foreach (DirStats Child in m_ChildList)
			{
				Child.SortChildStats(SortType, ReverseSort);
			}
		}
	}
}
