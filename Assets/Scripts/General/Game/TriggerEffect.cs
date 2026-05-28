using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TriggerEffect : MonoBehaviour
{
    public string ID;
    public float duration = 10f;

    Coroutine EffectRoutine;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Effect(other.gameObject);
    }

    public void SetID(string id)
    {
        ID = id;
    }

    public void Effect(GameObject player)
    {
        if(EffectRoutine != null) return;
        EffectRoutine = StartCoroutine(AddEffect(player));
    }

    IEnumerator AddEffect(GameObject player)
    {
        yield return new WaitForEndOfFrame();
        
         switch (ID)
        {
            case "Confuse":     EffectManager.Instance.Confuse(player, duration);    break;
            case "Guilt":       EffectManager.Instance.Guilt(player, duration);      break;
            case "ectoplasm":   EffectManager.Instance.Ectoplasm(player, duration);  break;
            case "transience":  EffectManager.Instance.Transience(player, duration); break;
            case "shapeless":   EffectManager.Instance.Shapeless(player, duration);  break;
            case "Overgrowth":  EffectManager.Instance.Overgrowth(player, duration); break;
            case "Polluted":    EffectManager.Instance.Polluted(player, duration);   break;
            case "Infection":   EffectManager.Instance.Infected(player, duration);   break;
            case "Ensare":      EffectManager.Instance.Ensare(player, duration);     break;
            case "PotOfGreed":  EffectManager.Instance.PotOfGreed(player);           break;
            default:
                break;
        }

        EffectRoutine = null;

    }

}