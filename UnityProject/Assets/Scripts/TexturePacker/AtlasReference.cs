using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AtlasReference : MonoBehaviour
{
    Dictionary<Image,AtlasData> m_refences = new Dictionary<Image, AtlasData>();
    public void AddRef(Image image, AtlasData ad)
    {
        if (m_refences.ContainsKey (image)) {
            --m_refences [image].ReferenceCount;
            m_refences [image] = ad;
        } else {
            m_refences.Add (image, ad);
        }

        ++ad.ReferenceCount;
    }

    void OnDestroy()
    {
        var iter = m_refences.GetEnumerator();
        while (iter.MoveNext ()) {
            --iter.Current.Value.ReferenceCount;
        }

        m_refences.Clear ();
    }
}
