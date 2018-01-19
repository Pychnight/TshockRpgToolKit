using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs.Invasions
{
	public delegate void InvasionStartHandler();
	public delegate void InvasionEndHandler();
	public delegate void InvasionUpdateHandler();
	public delegate void InvasionWaveStartHandler(int waveIndex, WaveDefinition waveDefinition);
	public delegate void InvasionWaveEndHandler(int waveIndex, WaveDefinition waveDefinition);
	public delegate void InvasionWaveUpdateHandler(int waveIndex, WaveDefinition waveDefinition, int currentPoints);
	public delegate void InvasionBossDefeatedHandler();
}
