using UnityEngine;

// Reseta flags estáticas de coleta no Awake da cena. Necessário porque
// KeyPickup.Collected sobrevive a SceneManager.LoadScene (RuntimeInitializeOnLoadMethod
// só dispara no boot do app). Sem isto, coletar a chave em Memory_02 e depois
// entrar em Memory_01 abriria a porta da escola sem matar o porteiro.
public class SceneStartReset : MonoBehaviour
{
    void Awake()
    {
        KeyPickup.ResetCollected();
        FoodInventory.ResetInventory();
    }
}
