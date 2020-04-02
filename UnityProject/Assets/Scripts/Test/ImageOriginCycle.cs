using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ImageOriginCycle : MonoBehaviour
{
    void Awake()
    {
        Image image = GetComponent<Image>();
        string fillOriginName = "";

        switch((Image.FillMethod)image.fillMethod)
        {
            case Image.FillMethod.Horizontal:
                fillOriginName = ((Image.OriginHorizontal)image.fillOrigin).ToString();
                break;
            case Image.FillMethod.Vertical:
                fillOriginName = ((Image.OriginVertical)image.fillOrigin).ToString();
                break;
            case Image.FillMethod.Radial90:

                fillOriginName = ((Image.Origin90)image.fillOrigin).ToString();
                break;
            case Image.FillMethod.Radial180:

                fillOriginName = ((Image.Origin180)image.fillOrigin).ToString();
                break;
            case Image.FillMethod.Radial360:
                fillOriginName = ((Image.Origin360)image.fillOrigin).ToString();
                break;
        }
        Debug.Log(string.Format("{0} is using {1} fill method with the origin on {2}", name, image.fillMethod, fillOriginName));
    }
}