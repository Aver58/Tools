
using UnityEngine.UI;

public class ImageEx : Image
{
    private AtlasReference m_AtlasRef;
    public AtlasReference GetAtlasReference()
    {
        if(m_AtlasRef == null)
        {
            m_AtlasRef = gameObject.GetComponentInParent<AtlasReference>();
        }
        return m_AtlasRef;
    }
}
