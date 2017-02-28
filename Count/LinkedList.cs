/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (LinkedList.cs) is part of Count.
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
 * LinkedList.cs - A generalized class that provides Linked List services.
 *    The class can be super-classed to provide more specific data storage.
 *    I wrote this class mostly as an exercise since the CLR already has a
 *    similar class. Also, it allowed me to include the ability to 
 *    efficiently sort the Linked list, in this case with a merge sort.
 */

using System;
using System.Collections;
using System.Text;

namespace Count
{
	class DOSLinkedList : IEnumerable, IEnumerator
	{
		// Author's note - in this case "DOS" is my initials, not the venerable OS.

		protected class DOSLinkedListNode
		{
			private object m_Data = null;

			public DOSLinkedListNode()
			{
			}

			public DOSLinkedListNode(object Data)
			{
				this.Data = Data;
			}

			public object Data
			{
				get
				{
					return m_Data;
				}
				set
				{
					m_Data = value;
				}
			}

			public DOSLinkedListNode Next
			{
				get;
				set;
			}

			public DOSLinkedListNode Previous
			{
				get;
				set;
			}
		}

		private DOSLinkedListNode m_Head = null;
		private DOSLinkedListNode m_Tail = null;
		private int m_NodeCount = 0;

		private DOSLinkedListNode m_EnumPos = null;
		private bool m_EnumReset = true;
		private bool m_EnumForwards = true;

		public DOSLinkedList()
		{
			//this.EnumerateListHeadToTail = true;
		}

		private void AddNode(object Data, bool AddLastIfTrue)
		{
			DOSLinkedListNode NewNode = new DOSLinkedListNode(Data);

			if (m_Head == null)
			{
				// empty list
				if (m_Tail != null)
					throw new CountException("Programming Error! Linked List in unstable state");

				m_Head = NewNode;
				m_Tail = NewNode;
				m_NodeCount = 1;
			}
			else
			{
				if (AddLastIfTrue == true)
				{
					// add last code
					NewNode.Previous = m_Tail;
					m_Tail.Next = NewNode;
					m_Tail = NewNode;
				}
				else
				{
					// add first code
					NewNode.Next = m_Head;
					m_Head.Previous = NewNode;
					m_Head = NewNode;
				}

				m_NodeCount++;
			}
		}

		protected virtual int CompareNodes(DOSLinkedListNode Node1, DOSLinkedListNode Node2)
		{
			// If Nodes carried ints, theoretical algorithm here would be 
			//  return Node1.Value - Node2.Value
			// Hence results are:
			//    postive int --> Node1 >  Node2
			//    zero        --> Node1 == Node2
			//    negative    --> Node1 <  Node2

			// higher class should override and do the compare, so we just return negative so nothing gets moved.
			return -1;
		}

		private DOSLinkedListNode Merge(DOSLinkedListNode Left, DOSLinkedListNode Right, out DOSLinkedListNode NewTail)
		{
			DOSLinkedListNode NewList = null;
			DOSLinkedListNode CurrentListNode = null;
			DOSLinkedListNode NodePick = null;

			if (Left == Right)
				Right = null;

			while ((Left != null) || (Right != null))
			{
				if (Left == null) // Case 1 - Left is empty, take from right.
				{
					NodePick = Right;
					Right = Right.Next;
				}
				else
				{
					if (Right == null) // Case 2 - Right is empty, take from left.
					{
						NodePick = Left;
						Left = Left.Next;
					}
					else
					{
						if (CompareNodes(Left, Right) < 0)  // Case 3 - Compare items to see which goes first.
						{
							// Left goes first
							NodePick = Left;
							Left = Left.Next;
						}
						else
						{
							NodePick = Right;
							Right = Right.Next;
						}
					}
				}

				// add item to NewList
				if (NewList == null)
				{
					NewList = NodePick;
					NodePick.Next = null;
					NodePick.Previous = null;
				}
				else
				{
					CurrentListNode.Next = NodePick;
					NodePick.Next = null;
					NodePick.Previous = CurrentListNode;
				}

				CurrentListNode = NodePick;
			}

			NewTail = CurrentListNode;
			return NewList;
		}

		private DOSLinkedListNode RecursiveMergeSort(DOSLinkedListNode List, int ListSize, out DOSLinkedListNode NewTail)
		{
			DOSLinkedListNode NewList = null;
			DOSLinkedListNode Left = List;
			DOSLinkedListNode Right = List;
			int i = 0;
			int ListHalf = (ListSize + 1) / 2;

			// make the 2 lists
			while ((Right != null) && (i < ListHalf))
			{
				Right = Right.Next;
				i++;
			}

			if ((Right != null) && (Right.Previous != null))
			{
				// separate left from right.
				Right.Previous.Next = null;
				Right.Previous = null;
			}

			// Tests - Left has an item, Left is not right (i.e. empty), 
			//  Left has more than one item (i.e. Next is not null)
			if ((Left != null) && (Left != Right) && (Left.Next != null))
				Left = RecursiveMergeSort(Left, ListHalf, out NewTail);

			// Tests - Right has an item, Right is not left, 
			//   Right has more than one item (i.e. Next is not null)
			if ((Right != null) && (Right != Left) && (Right.Next != null))
				Right = RecursiveMergeSort(Right, ListHalf, out NewTail);

			// merge lists
			NewList = Merge(Left, Right, out NewTail);

			return NewList;
		}

		private void MergeSort()
		{
			// if list has more than 1 item, then sort it, otherwise our work is done for us.
			if (m_NodeCount > 1)
				m_Head = RecursiveMergeSort(m_Head, m_NodeCount, out m_Tail);
		}

		public int Count
		{
			get
			{
				return m_NodeCount;
			}
		}

		public virtual void AddLast(object Data)
		{
			this.AddNode(Data, true);
		}

		public virtual void AddFirst(object Data)
		{
			this.AddNode(Data, false);
		}

		public bool IsTail(object CurItem)
		{
			if ((m_Tail != null) && (m_Tail.Data == CurItem))
				return true;

			return false;
		}

		public void Clear()
		{
			DOSLinkedListNode m_Cur = m_Head;
			DOSLinkedListNode m_Tmp = null;

			while (m_Cur != null)
			{
				m_Tmp = m_Cur;
				m_Cur = m_Cur.Next;

				m_Tmp.Previous = null;
				m_Tmp.Next = null;
			}

			m_Tmp = null;

			m_Head = null;
			m_Tail = null;
			m_NodeCount = 0;
		}

		public virtual void SortList()
		{
			MergeSort();
		}

		public bool EnumerateListHeadToTail
		{
			get
			{
				return m_EnumForwards;
			}
			set
			{
				m_EnumForwards = value;
			}
		}

		public virtual IEnumerator GetEnumerator()
		{
			// this seems like a hack, also this enumerator is NOT thread safe!
			m_EnumReset = true;
			m_EnumPos = null;

			return (IEnumerator)this;
		}

		public object Current
		{
			get
			{
				if (m_EnumPos != null)
					return m_EnumPos.Data;
				else
					throw new IndexOutOfRangeException(); //CountException("Attempt to read past end of list!");
			}
		}

		public bool MoveNext()
		{
			if (m_EnumReset == true)
			{
				if (m_EnumForwards == true)
					m_EnumPos = m_Head;
				else
					m_EnumPos = m_Tail;

				m_EnumReset = false;
			}
			else
			{
				if (m_EnumPos != null)
				{
					if (m_EnumForwards == true)
						m_EnumPos = m_EnumPos.Next;
					else
						m_EnumPos = m_EnumPos.Previous;
				}
			}

			if (m_EnumPos != null)
				return true;
			else
				return false;

		}

		public void Reset()
		{
			m_EnumReset = true;
			m_EnumPos = null;
		}

	}
}
