using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundTest
{
	/// <summary>
	/// Turns a ISampleGenerator into a stream
	/// </summary>
	public class WavStream : Stream
	{
		public WavStream(ISampleGenerator generator)
		{
			_channels = generator.Channels;
			_rate = generator.Frequency;
			_gen = generator;
			_totalSamples = _gen.TotalSamples; //caching this
			InitWaveHeader();
		}

		int _channels;
		int _rate;
		const int _bits = 16; //only supporting 16bits for now
		byte[] _wavHeader = null;
		int _headpos = 0;
		ISampleGenerator _gen;
		long _pos = 0;
		long _totalSamples;

		public override bool CanRead { get { return true; }}
		public override bool CanSeek { get { return false; }}
		public override bool CanWrite { get { return false; }}
		public override long Length { get {
			return _wavHeader.Length + SampleCountToByteCount(_totalSamples);
		}}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Position { get {
			if (_headpos < _wavHeader.Length) {
				return _headpos;
			} else {
				return _wavHeader.Length + SampleCountToByteCount(_pos);
			}
		} set {
			throw new NotImplementedException();
		}}

		public override int Read(byte[] buffer, int offset, int bytecount)
		{
			int remaining = 0;
			if (_headpos < _wavHeader.Length)
			{
				remaining = Math.Min(bytecount,_wavHeader.Length - _headpos);
				Buffer.BlockCopy(_wavHeader,_headpos,buffer,offset,remaining);
				_headpos += remaining;
				bytecount -= remaining;
				offset += remaining;
			}
			int samplecount = (int)Math.Min((long)ByteCountToSampleCount(bytecount),_totalSamples - _pos);
			if (samplecount > 0)
			{
				for(int s=0; s<samplecount; s++)
				{
					double samp = _gen.GetSample(_pos);
					_pos++;

					short scaled = (short)(samp * 0x7FF8); //32760
					//TODO only supporting 16 bit PCM so hardcoding the bytes
					buffer[offset+s*2] = (byte)(scaled & 0xFF);
					buffer[offset+s*2+1] = (byte)(scaled >> 8);

					//32-bit float format
					//Buffer.BlockCopy(sbuff, 0, buffer, offset, num); //this is kind of magic - copy a float array into a byte array
				}
			}

			return remaining + SampleCountToByteCount(samplecount);
		}

		//copied from NVorbis.NAudioSupport.VorbisFileReader.Read()
		//public override int Read(byte[] buffer, int offset, int bytecount)
		//{
		//	int headread = 0, num = 0;
		//	if (_headpos < _wavHeader.Length) {
		//		int remaining = Math.Min(bytecount - (int)_headpos,_wavHeader.Length - (int)_headpos);
		//		Buffer.BlockCopy(_wavHeader,(int)_headpos,buffer,offset,remaining);
		//		bytecount -= remaining;
		//		offset += remaining;
		//		_headpos += remaining;
		//		headread += remaining;
		//	}

		//	if (bytecount > 0) {
		//		int samplecount = ByteCountToSampleCount(bytecount);
		//		if (_fbuffer == null || _fbuffer.Length < samplecount) {
		//			_fbuffer = new float[samplecount];
		//		}
		//		for(int s=0; s<samplecount; s++)
		//		{
		//			_gen.GetSample(s);
		//		}
		//		int fsamp = _gen.ReadSamples(fbuffer, 0, samplecount);
		//		num = SampleCountToByteCount(fsamp);

		//		for(int f=0; f<fsamp; f++) {
		//			float samp = fbuffer[f];
		//			short scaled = (short)(samp * 0x8000);
		//			//TODO only supporting 16 bit PCM so hardcoding the bytes
		//			buffer[offset+f*2] = (byte)(scaled & 0xFF);
		//			buffer[offset+f*2+1] = (byte)(scaled >> 8);
		//		}
					
		//		//32-bit float format
		//		//Buffer.BlockCopy(sbuff, 0, buffer, offset, num); //this is kind of magic - copy a float array into a byte array
		//	}
		//	return headread + num;
		//}

		private int SampleCountToByteCount(int samplecount)
		{
			return (int)SampleCountToByteCount((long)samplecount);
		}
		private int ByteCountToSampleCount(int bytecount)
		{
			return (int)ByteCountToSampleCount((long)bytecount);
		}
		private long SampleCountToByteCount(long samplecount)
		{
			return samplecount * 2;
		}
		private long ByteCountToSampleCount(long bytecount)
		{
			bytecount /= 2;
			bytecount -= bytecount % _channels; //remove traling samples
			return bytecount;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
			//if (offset < _wavHeader.Length)
			//{
			//	headpos = offset;
			//	source.DecodedPosition = 0;
			//	return offset;
			//}
			//else
			//{
			//	offset -= wavHeader.Length;
			//	long soffset = ByteCountToSampleCount(offset);
			//	if (origin == SeekOrigin.Current) {
			//		soffset += source.DecodedPosition;
			//	} else if (origin == SeekOrigin.End) {
			//		soffset = source.TotalSamples - soffset;
			//	}
			//	source.DecodedPosition = soffset;
			//	return wavHeader.Length + SampleCountToByteCount(soffset);
			//}
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		//https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
		private void InitWaveHeader()
		{
			if (_wavHeader != null) { return; }

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			long len = SampleCountToByteCount(_gen.TotalSamples);

			bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
			bw.Write((int)(36 + len));
			bw.Write(new char[4] { 'W', 'A', 'V', 'E' });

			bw.Write(new char[4] { 'f', 'm', 't', ' ' });
			bw.Write((int)16);
			bw.Write((short)1);
			bw.Write((short)_channels);
			bw.Write((int)_rate);
			bw.Write((int)(_rate * ((16 * _channels) / 8)));
			bw.Write((short)((16 * _channels) / 8));
			bw.Write((short)16);

			bw.Write(new char[4] { 'd', 'a', 't', 'a' });
			bw.Write((int)len);
				
			_wavHeader = ms.ToArray();
		}
	}
}
