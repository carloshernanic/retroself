using UnityEngine;

// Base abstrata pra qualquer "fonte" que alimenta um GatedDoor. Permite plugar
// PressurePlate (ocupado por player/HeavyBox) e StoneSwitch (acertado por pedra)
// na mesma lista de sources do gate. Sem isso, GatedDoor.sources teria que ser
// uma interface ou lista por tipo concreto — e Unity não serializa List<interface>.
public abstract class GateSource : MonoBehaviour
{
    public abstract bool IsActive { get; }
}
