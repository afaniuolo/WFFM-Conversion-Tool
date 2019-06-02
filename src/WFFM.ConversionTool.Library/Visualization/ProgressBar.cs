using System;

namespace WFFM.ConversionTool.Library.Visualization
{
	public class ProgressBar
	{
		public static void DrawTextProgressBar(int progress, int total, string progressMessage)
		{
			//draw empty progress bar
			Console.CursorLeft = 2;
			Console.Write("["); //start
			Console.CursorLeft = 34;
			Console.Write("]"); //end
			Console.CursorLeft = 3;
			float onechunk = 30.0f / total;

			//draw filled part
			int position = 3;
			for (int i = 0; i < onechunk * progress; i++)
			{
				Console.BackgroundColor = ConsoleColor.Green;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw unfilled part
			for (int i = position; i <= 33; i++)
			{
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw totals
			Console.CursorLeft = 37;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Write($"{progress} of {total} {progressMessage}   "); //blanks at the end remove any excess
		}
	}
}
