using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundGen
{
	// http://www.lomont.org/Software/Misc/Robotron/
	public class WilliamsSound
	{
		/// <summary>
		/// This function reproduces an algorithm from the Williams Sound ROM, addresses 0xF503 to 0xF54F
		/// It takes 7 byte parameters and one 16-bit parameter, and returns a list of sound values 0-255
		/// sampled at 894750 samples per second
		/// </summary>
		public static List<byte> Sound1(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, ushort u1)
		{
			ushort count;   // internal counter
			byte c1, c2; // internal storage
			byte sound = 0; // current sound level
			var wave = new List<byte>();
			// copy the current sound value this many times into the output
			Action<int> dup = d =>
									{
										while (d-- > 0)
											wave.Add(sound);
									};
 
			dup(8);
			sound = b7;
			do
			{
				dup(14);
				c1 = b1;
				c2 = b2;
				do
				{
					dup(4);
					count = u1;
					while (true)
					{
						dup(9);
						sound = (byte) ~sound;
 
						ushort t1 = (c1 != 0 ? c1 : (ushort)256);
						dup(Math.Min(count, t1)*14 - 6);
						if (count <= t1)
							break;
						dup(12);
						count -= t1;
 
						sound = (byte) ~sound;
 
						ushort t2 = (c2 != 0 ? c2 : (ushort)256);
						dup(Math.Min(count, t2) * 14 - 3);
						if (count <= t2)
							break;
						dup(10);
						count -= t2;
					}
 
					dup(15);
 
					if (sound < 128)
					{
						dup(2);
						sound = (byte)~sound;
					}
 
					dup(27);
					c1 += b3;
					c2 += b4;
				} while (c2 != b5);
 
				dup(7);
				if (b6 == 0) break;
				dup(11);
				b1 += b6;
			} while (b1 != 0);
			return wave;
		}
	}
}