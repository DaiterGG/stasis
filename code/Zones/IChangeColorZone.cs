using System;

namespace Stasis.Zones;
public interface IChangeColorZone : IZone 
{
	[Property, Description("Change Color On Trigger")]
	bool ChangeColor { get; set; }
	[Property, ShowIf("ChangeColor", true)]
	Color ColorBefore {get; set;}
	[Property, ShowIf("ChangeColor", true)]
	Color ColorAfter {get; set;}
	public void ActivateToggle(bool activate = true);
	public void ColorUpdate();
}
