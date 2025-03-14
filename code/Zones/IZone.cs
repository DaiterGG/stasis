namespace Stasis.Zones;
public interface IZone
{
    void OnTriggerEnter( Collider col );
    int ID { get; set; }
    int Locked { get; set; }
}
