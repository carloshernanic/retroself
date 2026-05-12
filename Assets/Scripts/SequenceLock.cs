using System.Collections.Generic;
using UnityEngine;

// Cadeado de sequência estilo Genius/Simon: o jogador deve acertar os switches
// coloridos numa ordem específica. Erros resetam todos os switches (cada um
// volta a ResetSwitch) e o progresso retorna a 0. Quando completa, fica latched
// pra sempre — o gate final pode usar isso como source única.
//
// Como GateSource, plugga direto em GatedDoor.sources. As pistas (números
// coloridos espalhados no mapa) revelam expectedOrder ao longo do level.
public class SequenceLock : GateSource
{
    public List<StoneSwitch> switches = new List<StoneSwitch>();
    public List<int> expectedOrder = new List<int>();

    public override bool IsActive => unlocked;

    private int progress;
    private bool unlocked;

    void Start()
    {
        for (int i = 0; i < switches.Count; i++)
        {
            int idx = i;
            var sw = switches[i];
            if (sw == null) continue;
            sw.OnHit.AddListener(() => HandleHit(idx));
        }
    }

    void HandleHit(int hitIndex)
    {
        if (unlocked) return;
        if (progress >= expectedOrder.Count) return;

        if (expectedOrder[progress] == hitIndex)
        {
            progress++;
            if (progress >= expectedOrder.Count) unlocked = true;
        }
        else
        {
            // Erro: resetar todos os switches e zerar progresso. O jogador tenta
            // de novo. Switches latched precisam ser destravados aqui — por isso
            // usamos non-latched no puzzle 4.
            progress = 0;
            foreach (var sw in switches) if (sw != null) sw.ResetSwitch();
        }
    }
}
