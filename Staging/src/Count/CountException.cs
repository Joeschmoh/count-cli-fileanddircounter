/*  
 *  Count - A file & directory counter with free trivial information!
 *  Copyright (C) 2011 Derick Snyder
 *  
 *  This file (CountException.cs) is part of Count.
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
 * CountException.cs - A simple wrapper exception class so that specific 
 *   Count errors can be trapped and displayed to the user.
 *
 */

using System;

namespace Count
{
	class CountException : Exception
	{
		public CountException()
			: base()
		{
		}

		public CountException(string message)
			: base(message)
		{
		}

		public CountException(string message, System.Exception inner)
			: base(message, inner)
		{
		}

		protected CountException(System.Runtime.Serialization.SerializationInfo info,
										 System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
	}
}
