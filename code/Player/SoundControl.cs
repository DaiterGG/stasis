namespace Sandbox.Player;

public sealed class SoundControl : Component
{
	SoundPointComponent SPC;
	EngineComponent ENGINE;
	SpinControl SPINC;
	public void OnAwakeInit()
	{
		SPC = GameObject.Components.Get<SoundPointComponent>();
		ENGINE = Sng.Inst.Player.Engine;
		SPINC = Sng.Inst.Player.SpinC;

	}
	public void OnFixedGlobal()
	{
		if ( ENGINE.isRunning )
		{
			var gain = ENGINE.gain / ENGINE.maxGain;

			SPC.Pitch = gain * gain;
			SPC.Volume = gain;
			if ( gain < 0.55f )
			{
				SPC.Pitch += 0.06f;
			}
			else if ( gain < 0.2f )
			{
				SPC.Volume = 0f;
			}
		}
		else
		{
			var prog = ENGINE.progress / 100f;
			SPC.Pitch = prog * prog;
			if ( prog < 0.3f ) SPC.Volume = prog - 0.1f;
			else
			{
				SPC.Volume = prog;

			}
		}
		if ( !SPINC.isAttached ) SPC.Volume = 0;
		SPC.Pitch = SPC.Pitch > 1f ? 1f : SPC.Pitch;
	}
}
