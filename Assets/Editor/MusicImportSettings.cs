#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Force AudioImporter settings on retroself-v1.wav: Streaming load type
// (arquivo é ~19MB, evita carregar tudo na RAM no boot).
public class MusicImportSettings : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.EndsWith("retroself-v1.wav")) return;
        var ai = (AudioImporter)assetImporter;
        var settings = ai.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.Streaming;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;
        ai.defaultSampleSettings = settings;
    }
}
#endif
