using UnityEngine;
using Schnibble;

public class GoldenTicketCollectible : AbstractCollectible
{
    public string trackname = Constants.SN_GROTTO_TRACK;
    public AudioSource collect_SFX;
    void Start()
    {
    }

    protected override void OnCollect()
    {
        if (null!=collect_SFX)
            Schnibble.Utils.SpawnAudioSource(collect_SFX, transform);
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}