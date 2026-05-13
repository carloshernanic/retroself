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
        public int lane;     // 0 = Young (esquerda), 1 = Adult (direita)
        public int column;   // 0 = A/seta-esquerda, 1 = S/seta-baixo, 2 = D/seta-direita
    }

    public List<Note> notes = new List<Note>();

    // Lead-in (silêncio/intro) do AudioClip antes do downbeat 1 da música. GH usa
    // pra alinhar songTime ao primeiro tempo real: songTime = songSource.time - songStartOffset.
    // Detectado em editor pelo BeatMapPlaceholderBuilder via onset-detection no WAV.
    public float songStartOffset = 0f;
}
