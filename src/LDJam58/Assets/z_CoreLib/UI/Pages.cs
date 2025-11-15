using UnityEngine;

public sealed class Pages : MonoBehaviour
{
    [SerializeField] private Page[] pages;
    // TODO: Add SoundType.PageTurn to AudioConfig and use AudioSystem here
    // [SerializeField] private SoundType pageTurnSound = SoundType.PageTurn;

    private IndexSelector<Page> _pages;
    
    void Awake()
    {
        _pages = new IndexSelector<Page>(pages);
        foreach (var p in pages)
        {
            p.Init(MovePrevious, MoveNext, PerformAction);
            p.gameObject.SetActive(false);
        }
        _pages.Current.gameObject.SetActive(true);
    }

    public void PerformAction()
    {
        _pages.Current.gameObject.SetActive(false);
        _pages.MoveNextWithoutLooping().gameObject.SetActive(true);
    }
    
    public void MoveNext()
    {
        _pages.Current.gameObject.SetActive(false);
        _pages.MoveNextWithoutLooping().gameObject.SetActive(true);
        // TODO: AudioSystem.Instance.Play(pageTurnSound, default);
    }

    public void MovePrevious()
    {
        _pages.Current.gameObject.SetActive(false);
        _pages.MovePrevious().gameObject.SetActive(true);
        // TODO: AudioSystem.Instance.Play(pageTurnSound, default);
    }
}
