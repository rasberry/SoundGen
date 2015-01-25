using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundTest
{
	/// <summary>
	/// helper interface that defines the contract for a plugin sample generator
	/// </summary>
	public interface ISampleGenerator
	{
		/// <summary>
		/// returns the sample for given index. 
		/// </summary>
		/// <param name="index">index number 0 to TotalSamples-1</param>
		/// <returns>should be between -1.0 and 1.0 or bit overflows happen</returns>
		double GetSample(long index);
		/// <summary>
		/// total number of samples.
		/// </summary>
		long TotalSamples { get; }
		/// <summary>
		/// number of channels. usually 2
		/// </summary>
		int Channels { get; }
		/// <summary>
		/// frequncy. usually 44100
		/// </summary>
		int Frequency { get; }
	}
}
