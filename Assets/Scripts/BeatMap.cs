using System.Collections.Generic;
using UnityEngine;

// Mapa de notas pra GuitarHeroMinigame. Lane 0 = Young (esquerda), 1 = Adult
// (direita). `time` em segundos desde o início da música. Ordenado por time.
//
// ScriptableObject pra permitir trocar a música/notas sem rebuild da cena —
// builder cria um asset placeholder e o usuário pode plugar uma versão real
// no Inspector da cabine do Guitar Hero.
[CreateAssetMenu(fileName = "BeatMap_NEW", menuName = "Retroself/Beat Map")]
public class BeatMap : ScriptableObject
{
    [System.Serializable]
    public struct Note
    {
        public float time;
        public int lane;
    }

    public List<Note> notes = new List<Note>();
}
