using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Game.Exhibits;

public class TagsDisplayView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tag1;
    [SerializeField] private TextMeshProUGUI _tag2;
    [SerializeField] private TextMeshProUGUI _tag3;

    public void SetTags(List<ExhibitTag> tags)
    {
        _tag1.gameObject.SetActive(tags.Count > 0);
        _tag2.gameObject.SetActive(tags.Count > 1);
        _tag3.gameObject.SetActive(tags.Count > 2);
        
        if (tags.Count > 0)
            _tag1.text = TmpIconName(tags[0]);
        if (tags.Count > 1)
            _tag2.text = TmpIconName(tags[1]);
        if (tags.Count > 2)
            _tag3.text = TmpIconName(tags[2]);
    }

    private string TmpIconName(ExhibitTag tag)
    {
        return "<sprite name=\"" + tag.ToString() + "\">";
    }
}
