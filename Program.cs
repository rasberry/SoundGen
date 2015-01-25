using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoundTest
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			//WavStream stream = new WavStream(new TestSoundOne());
			//WavStream stream = new WavStream(new TestSoundTwo());
			//WavStream stream = new WavStream(new TestSoundThree());
			WavStream stream = new WavStream(new TestSoundFour());
			//var s = new System.Media.SoundPlayer(stream); s.PlaySync();
			//DebugStream(stream);
			SaveToFile(stream,"Four"+DateTime.Now.ToString("yyyyMMddhhmmss")+".wav");
		}

		static void SaveToFile(Stream stream, string name)
		{
			using (var writer = File.Open(name,FileMode.Create,FileAccess.Write,FileShare.Read)) {
				stream.CopyTo(writer);
			}
		}

		static void DebugStream(Stream stream)
		{
			var hex = new System.Text.StringBuilder((int)stream.Length * 2);
			using(var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				byte[] data = memoryStream.ToArray();
				foreach (byte b in data) {
					hex.AppendFormat("{0:x2} ", b);
				}
				Console.WriteLine(hex.ToString());
			}
		}

		static string ByteArrayToString(byte[] ba)
		{
			var hex = new System.Text.StringBuilder(ba.Length * 2);
			foreach (byte b in ba) {
				hex.AppendFormat("{0:x2} ", b);
			}
			return hex.ToString();
		}
	}
}
