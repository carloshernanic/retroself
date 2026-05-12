#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Helpers compartilhados pelos builders pra que rebuilds **não apaguem** edições
// manuais que o usuário fez na cena (Light2D, Volumes extras, GameObjects de
// decoração que adicionou no editor). Em vez de criar uma cena vazia a cada
// rebuild (NewScene/EmptyScene), abre a cena existente e destrói **só** os
// roots cujos nomes o builder gerencia. Tudo fora dessa lista sobrevive.
//
// Convenção pra o usuário: ao adicionar luzes/Volumes manuais, deixe-os como
// roots da cena (ou filhos de um GameObject de nome livre tipo "_UserExtras").
// Não pendure como filho de "Lamp"/"Bench"/etc., porque esses são gerenciados
// pelo builder e somem no rebuild.
public static class SceneRebuildHelpers
{
    public static Scene OpenOrNew(string scenePath)
    {
        // Se a cena alvo já está aberta no editor, **usa direto** sem reabrir.
        // OpenScene recarrega do disco e descartaria edições não salvas (luzes
        // adicionadas, tweaks no Inspector). Esse caminho preserva o trabalho
        // em progresso do usuário.
        var active = EditorSceneManager.GetActiveScene();
        if (active.IsValid() && active.path == scenePath)
            return active;

        if (System.IO.File.Exists(scenePath))
        {
            // Pergunta antes de descartar mudanças em outras cenas abertas.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return active; // usuário cancelou
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    // Mostra um diálogo listando o que o rebuild vai mexer e o que vai preservar.
    // Retorna false se o usuário cancelou. O builder deve abortar nesse caso.
    public static bool ConfirmRebuild(Scene scene, string[] ownedNames)
    {
        var willReplace = new System.Collections.Generic.List<string>();
        var willKeep = new System.Collections.Generic.List<string>();
        int liftedLights = 0, liftedVolumes = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (System.Array.IndexOf(ownedNames, root.name) >= 0)
            {
                willReplace.Add(root.name);
                liftedLights += root.GetComponentsInChildren<Light2D>(true).Length;
                liftedVolumes += root.GetComponentsInChildren<Volume>(true).Length;
            }
            else
            {
                willKeep.Add(root.name);
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("VAI SUBSTITUIR (recriado do zero):");
        if (willReplace.Count == 0) sb.AppendLine("  - (nada)");
        else foreach (var n in willReplace) sb.AppendLine("  - " + n);

        sb.AppendLine();
        sb.AppendLine("MANTÉM INTACTO (não toca):");
        if (willKeep.Count == 0) sb.AppendLine("  - (nada)");
        else foreach (var n in willKeep) sb.AppendLine("  - " + n);

        if (liftedLights + liftedVolumes > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"LIFTED PRA SCENE ROOT (preservados):");
            if (liftedLights > 0)  sb.AppendLine($"  - {liftedLights} Light2D aninhados");
            if (liftedVolumes > 0) sb.AppendLine($"  - {liftedVolumes} Volume(s) aninhados");
        }

        sb.AppendLine();
        sb.AppendLine("Profile do post-processing (.asset) é preservado.");

        return EditorUtility.DisplayDialog("Confirmar rebuild", sb.ToString(), "Continuar", "Cancelar");
    }

    public static void WipeOwnedRoots(Scene scene, string[] ownedNames)
    {
        var roots = scene.GetRootGameObjects();
        int preservedLights = 0, preservedVolumes = 0;
        foreach (var root in roots)
        {
            if (System.Array.IndexOf(ownedNames, root.name) < 0) continue;

            // Antes de destruir o root, salva qualquer Light2D ou Volume aninhado:
            // unparent pra scene root mantendo world transform. O usuário costuma
            // colocar a luz como filha do "Lamp" ou da "Main Camera" — esses morrem
            // no rebuild, mas a luz sobrevive lifted pra root.
            preservedLights += PreserveDescendants<Light2D>(root);
            preservedVolumes += PreserveDescendants<Volume>(root);

            Object.DestroyImmediate(root);
        }
        if (preservedLights + preservedVolumes > 0)
            Debug.Log($"[SceneRebuildHelpers] preservou {preservedLights} Light2D + {preservedVolumes} Volume(s) lifted pra scene root.");
    }

    static int PreserveDescendants<T>(GameObject root) where T : Component
    {
        // GetComponentsInChildren inclui o próprio root. Para cada match, sobe pra
        // scene root (worldPositionStays = true mantém posição visual).
        var found = root.GetComponentsInChildren<T>(includeInactive: true);
        foreach (var c in found)
        {
            if (c == null) continue;
            c.transform.SetParent(null, worldPositionStays: true);
        }
        return found.Length;
    }
}
#endif
