using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{

    public PlayerControlRigid playerControl;
    public MagicManagement playerMagic;
    public HP playerHealth;
    public Launcher playerSpell;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI magicText;
    public TextMeshProUGUI spellText;

    public TextMeshProUGUI hpText;

    public Animator heartAnim;

    public Sprite[] speedFrames;

    public SpriteRenderer speedRender;

    public SpriteRenderer blastRender;

    void Update()
    {

        hpText.text = playerHealth.currentHP.ToString("F0");

        // speedText.text = playerControl.playerSpeed.ToString("F0");

        magicText.text = playerMagic.magicPoints.ToString("F0");

        spellText.text = playerSpell.spell.ToString("F0");

        // if (playerMagic.justUsed)
        // {
        //     StartCoroutine(Blast());
        // }
    }

    // IEnumerator Blast()
    // {
    //     if (flashing) yield break;
    //     flashing = true;

    //     blastRender.color = new Color(1f, 1f, 1f, 1f);

    //     yield return new WaitForSeconds(0.5f);

    //     blastRender.color = new Color(1f, 1f, 1f, 0f);

    //     flashing = false;
    // }
}