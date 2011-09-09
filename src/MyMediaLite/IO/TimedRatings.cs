// Copyright (C) 2010, 2011 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers methods for reading in rating data</summary>
	public static class TimedRatings
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings Read(string filename, IEntityMapping user_mapping, IEntityMapping item_mapping)
		{
			using ( var reader = new StreamReader(filename) )
				return Read(reader, user_mapping, item_mapping);
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings
			Read(TextReader reader,	IEntityMapping user_mapping, IEntityMapping item_mapping)
		{
			var ratings = new MyMediaLite.Data.TimedRatings();

			var split_chars = new char[]{ '\t', ' ', ',' };
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(split_chars);

				if (tokens.Length < 4)
					throw new IOException("Expected at least 4 columns: " + line);

				int user_id = user_mapping.ToInternalID(int.Parse(tokens[0]));
				int item_id = item_mapping.ToInternalID(int.Parse(tokens[1]));
				double rating = double.Parse(tokens[2], CultureInfo.InvariantCulture);
				string date_string = tokens[3];
				if (tokens[3].StartsWith("\"") && tokens.Length > 4 && tokens[4].EndsWith("\""))
				{
					date_string = tokens[3] + " " + tokens[4];
					date_string = date_string.Substring(1, date_string.Length - 2);
				}

				DateTime time = DateTime.Parse(date_string, CultureInfo.InvariantCulture);
				ratings.Add(user_id, item_id, rating, time);

				if (ratings.Count % 20000 == 19999)
					Console.Error.Write(".");
				if (ratings.Count % 1200000 == 1199999)
					Console.Error.WriteLine();
			}
			return ratings;
		}
	}
}