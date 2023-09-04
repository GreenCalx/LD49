using UnityEngine;
using Schnibble;

public class CageKeyCollectible : AbstractCollectible
{
    public string trackname = "";
    public AudioSource keyCollect_SFX;
    void Start()
    {
        if (Access.CollectiblesManager().hasCageKey(trackname))
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnCollect()
    {
        Schnibble.Utils.SpawnAudioSource(keyCollect_SFX, transform);
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}
