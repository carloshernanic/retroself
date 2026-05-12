#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Gera AnimatorControllers + AnimationClips programaticamente pros 3
// personagens (WoodyYoung, WoodyAdult, Porteiro). Cada controller tem
// states Idle/Walk/Jump com sprite-curve apontando pro Body do GameObject.
public static class AnimatorBuilder
{
    const string AnimDir = "Assets/Animations";

    [MenuItem("Retroself/Build Character Animators")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(AnimDir))
            AssetDatabase.CreateFolder("Assets", "Animations");

        // WoodyYoung: Child_3 não tem Jump.png, então o state Jump usa o clip de Idle.
        BuildController(
            "WoodyYoung",
            idleSheet: "Assets/Sprites/criancas/Child_3/Idle.png",
            walkSheet: "Assets/Sprites/criancas/Child_3/Walk.png",
            jumpSheet: "Assets/Sprites/criancas/Child_3/Idle.png",
            jumpLoop: true);

        BuildController(
            "WoodyAdult",
            idleSheet: "Assets/Sprites/mendigos/Homeless_1/Idle.png",
            walkSheet: "Assets/Sprites/mendigos/Homeless_1/Walk.png",
            jumpSheet: "Assets/Sprites/mendigos/Homeless_1/Jump.png",
            jumpLoop: false);

        BuildController(
            "Porteiro",
            idleSheet: "Assets/Sprites/gangsters/Gangsters_2/Idle.png",
            walkSheet: "Assets/Sprites/gangsters/Gangsters_2/Walk.png",
            jumpSheet: "Assets/Sprites/gangsters/Gangsters_2/Jump.png",
            jumpLoop: false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[AnimatorBuilder] Controllers gerados em " + AnimDir);
    }

    static void BuildController(string name, string idleSheet, string walkSheet, string jumpSheet, bool jumpLoop)
    {
        string controllerPath = $"{AnimDir}/{name}.controller";

        // Cria/recria o controller do zero.
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalVel", AnimatorControllerParameterType.Float);

        // Garante que IsGrounded começa true pra Animator não disparar Jump no spawn.
        var paramList = new List<AnimatorControllerParameter>(controller.parameters);
        var grounded = paramList.Find(p => p.name == "IsGrounded");
        if (grounded != null) grounded.defaultBool = true;
        controller.parameters = paramList.ToArray();

        var sm = controller.layers[0].stateMachine;

        var idleClip = BuildClip(name, "Idle", idleSheet, fps: 10f, loop: true);
        var walkClip = BuildClip(name, "Walk", walkSheet, fps: 12f, loop: true);
        var jumpClip = BuildClip(name, "Jump", jumpSheet, fps: 8f, loop: jumpLoop);

        var idleState = sm.AddState("Idle", new Vector3(280, 100));
        idleState.motion = idleClip;
        var walkState = sm.AddState("Walk", new Vector3(280, 200));
        walkState.motion = walkClip;
        var jumpState = sm.AddState("Jump", new Vector3(520, 150));
        jumpState.motion = jumpClip;

        sm.defaultState = idleState;

        // Idle <-> Walk via Speed
        AddTransition(idleState, walkState, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddTransition(walkState, idleState, "Speed", AnimatorConditionMode.Less, 0.1f);

        // AnyState -> Jump quando IsGrounded == false
        var toJump = sm.AddAnyStateTransition(jumpState);
        toJump.hasExitTime = false;
        toJump.duration = 0.05f;
        toJump.canTransitionToSelf = false;
        toJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

        // Jump -> Idle/Walk quando voltar ao chão
        var jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.hasExitTime = false;
        jumpToIdle.duration = 0.05f;
        jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        var jumpToWalk = jumpState.AddTransition(walkState);
        jumpToWalk.hasExitTime = false;
        jumpToWalk.duration = 0.05f;
        jumpToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        EditorUtility.SetDirty(controller);
    }

    static void AddTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.AddCondition(mode, threshold, param);
    }

    static AnimationClip BuildClip(string charName, string stateName, string sheetPath, float fps, bool loop)
    {
        string clipPath = $"{AnimDir}/{charName}_{stateName}.anim";
        var sprites = LoadFrames(sheetPath);
        if (sprites.Count == 0)
        {
            Debug.LogWarning($"[AnimatorBuilder] sem frames em {sheetPath} — clip {clipPath} vazio");
        }

        var clip = new AnimationClip { frameRate = fps };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "Body",
            propertyName = "m_Sprite",
        };

        var keys = new ObjectReferenceKeyframe[Mathf.Max(1, sprites.Count)];
        if (sprites.Count == 0)
        {
            keys[0] = new ObjectReferenceKeyframe { time = 0f, value = null };
        }
        else
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprites[i],
                };
            }
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    static List<Sprite> LoadFrames(string sheetPath)
    {
        var list = new List<Sprite>();
        // LoadAllAssetRepresentationsAtPath devolve só os sub-assets (slices),
        // nunca o main Texture2D ou o sprite whole-texture que sobra em Single mode.
        // É a API correta pra ler só os frames sliced de uma textura Multiple.
        var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
        string baseName = System.IO.Path.GetFileNameWithoutExtension(sheetPath);
        string prefix = baseName + "_";
        foreach (var obj in subs)
        {
            if (obj is Sprite s && s.name.StartsWith(prefix))
                list.Add(s);
        }
        // Ordena pelo índice numérico no sufixo "_N" (lex sort coloca _10 entre _1 e _2).
        list.Sort((a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));
        Debug.Log($"[AnimatorBuilder] {sheetPath} → {list.Count} slices: [{string.Join(", ", list.ConvertAll(s => s.name))}]");
        return list;
    }

    static int ExtractIndex(string spriteName)
    {
        int u = spriteName.LastIndexOf('_');
        if (u < 0 || u + 1 >= spriteName.Length) return 0;
        return int.TryParse(spriteName.Substring(u + 1), out var i) ? i : 0;
    }
}
#endif
