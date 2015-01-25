using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundGen
{
	class TestSoundFour : ISampleGenerator
	{
		public double GetSample(long index)
		{
			if (--rem < 1) {
				int key = rnd.Next(1,89);
				space = rnd.NextDouble() < 0.125;
				freq = GetPianoFreq(key);
				rem = rnd.Next(Frequency/8,Frequency);
				//Console.WriteLine(key+"\t"+space+"\t"+freq+"\t"+rem);
				att = attack;
			}
			double amp = att > 0 ? (attack - (--att))/attack : 1.0
				+ rem < attack ? rem/attack : 1.0;

			if (space) {
				return 0.0;
			} else {
				//return a sine wave
				//return amp * Math.Sin(Math.PI * index * freq / Frequency);

				//atempt at a piano waveform : http://people.uwec.edu/walkerjs/media/38228[1].pdf
				double final =
					  1.00 * amp * Math.Sin(Math.PI * index * 1.0 * freq / Frequency)
					+ 0.24 * amp * Math.Sin(Math.PI * index * 0.5 * freq / Frequency)
					+ 0.10 * amp * Math.Sin(Math.PI * index * 1.5 * freq / Frequency)
					+ 0.15 * amp * Math.Sin(Math.PI * index * 2.5 * freq / Frequency)
				;
				return final / 1.49;
			}
		}
		const double attack = 44100/32;
		double att = 0;
		int rem = 0;
		double freq = 0;
		bool space = false;
		Random rnd = new Random();

		const long len = 44100 * 32;

		public long TotalSamples { get { return len; }}
		public int Channels { get { return 2; }}
		public int Frequency { get { return 44100; }}

		//key 1 - 88
		static double GetPianoFreq(int key)
		{
			//f=2^((n-49)/12)*440;
			return 440.0*Math.Pow(2,(key-49.0)/12.0);
		}
	}

	class TestSoundThree : ISampleGenerator
	{
		const int num = 4;
		public TestSoundThree()
		{
			//_start = new double[] { 0800.0,0400.0,0100.0 };
			//_end = new double[]   { 0600.0,2000.0,4000.0 };
			//_start = new double[] { 5000.0, 0500.0 };
			//_end = new double[]   { 0100.0, 1000.0 };
			//_start = new double[] { 0100.0,0100.0,0100.0,0100.0 };
			//_end = new double[]   { 0200.0,0400.0,0500.0,0600.0 };
			var rnd = new Random();
			_start = new double[num];
			_end = new double[num];
			for(int n=0; n<num; n++) {
				_start[n] = rnd.NextDouble()*2000;
				_end[n] = rnd.NextDouble()*2000;
			}

			_mx = new double[_start.Length];
			calcMX();
		}
		double[] _start;
		double[] _end;
		const int len = 44100*32;
		double[] _mx;

		void calcMX()
		{
			for(int s=0; s<_start.Length; s++)
			{
				_mx[s] = (_end[s] - _start[s])/len/Channels;
			}
		}

		public double GetSample(long index)
		{
			double final = 0.0;
			for(int s=0; s<_start.Length; s++)
			{
				double f = (double)index * _mx[s] + _start[s];
				final += Math.Sin(Math.PI * index * f / Frequency);
			}
			return final / _start.Length;
		}

		public long TotalSamples { get { return len; }}
		public int Channels { get { return 2; }}
		public int Frequency { get { return 44100; }}
	}

	class TestSoundTwo : ISampleGenerator
	{
		public TestSoundTwo()
		{
			//_data = WilliamsSound.Sound1(0x40,0x01,0x00,0x10,0xE1,0xFF,0xFF,0x0080);
			//_data = WilliamsSound.Sound1(0x28,0x01,0x00,0x08,0x81,0xFF,0xFF,0x0200);
			_data = WilliamsSound.Sound1(0x28,0x81,0x00,0xFC,0x01,0xFC,0xFF,0x0200);
			//_data = WilliamsSound.Sound1(0xFF,0x01,0x00,0xFF,0x41,0x00,0xFF,0x0480);
			//_data = WilliamsSound.Sound1(0x00,0xFF,0x08,0xFF,0x68,0x00,0xFF,0x0480);
			//_data = WilliamsSound.Sound1(0x20,0xFF,0x10,0xFF,0x18,0xFE,0x00,0x0480);

		}
		List<byte> _data;

		const int downsample = 8;
		const int originalRate = 894750;
		public double GetSample(long index)
		{
			//signal decimation.. just skip n samples
			//if (index*downsample < _data.Count)
			//{
			//	return (double)_data[(int)index*downsample]/255.0;
			//}
			//signal averaging.. take n samples and average them
			if (index+downsample < _data.Count)
			{
				double avg = 0.0;
				for (int s=0; s<downsample;s++) {
					avg += (double)_data[(int)index*downsample+s]/255.0;
				}
				return avg/downsample;
			}
			return 0.0;
		}

		public long TotalSamples { get { return _data.Count/downsample; }}
		public int Channels { get { return 2; }}
		public int Frequency { get { return 44100; }}
	}

	class TestSoundOne : ISampleGenerator
	{
		public double GetSample(long index)
		{
			double f1 = (index / 44100.0 * 32.0) + 100.0;
			return Math.Sin(Math.PI * f1 * index / 44100);
		}

		public long TotalSamples { get { return 44100*2; }}
		public int Channels { get { return 2; }}
		public int Frequency { get { return 44100; }}
	}
}
