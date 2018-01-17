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
	public delegate void WaveStartHandler(int waveIndex, WaveDefinition wave);
	public delegate void WaveEndHandler(int a, WaveDefinition waveDefinition);
	public delegate void WaveUpdateHandler(int a, WaveDefinition waveDefinition, int b);
	public delegate void BossDefeatedHandler();
}
