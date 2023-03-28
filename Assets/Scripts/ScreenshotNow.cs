using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;


public class ScreenshotNow : MonoBehaviour 
{
    public RectTransform rectT; // Assign the UI element which you wanna capture
    public Image img;
    int width; // width of the object to capture
    int height; // height of the object to capture

    // Use this for initialization
    void Start () 
    {
        width = System.Convert.ToInt32(rectT.rect.width); 
        height = System.Convert.ToInt32(rectT.rect.height);
    }
	
    // Update is called once per frame
    void Update () 
    {
        if (Input.GetMouseButtonDown (0)) 
        {
            //StartCoroutine(takeScreenShot ()); // screenshot of a particular UI Element.
        }
        if (Input.GetKeyDown (KeyCode.A)) 
        {
            ScreenCapture.CaptureScreenshot("ScreenShot.png");
        }
	
    }
    public IEnumerator takeScreenShot()
    {
        yield return new WaitForEndOfFrame (); // it must be a coroutine 

        Vector2 temp = rectT.transform.position;
        var startX = temp.x - width/2;
        var startY = temp.y - height/2;

        var tex = new Texture2D (width, height, TextureFormat.RGB24, false);
        tex.ReadPixels (new Rect(startX, startY, width, height), 0, 0);
        tex.Apply ();

        // Encode texture into PNG
        var bytes = tex.EncodeToPNG();
        Destroy(tex);

        //File.WriteAllBytes(Application.dataPath + "ScreenShot.png", bytes);

        string imgsrc = System.Convert.ToBase64String(bytes);
        Texture2D scrnShot = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        scrnShot.LoadImage(System.Convert.FromBase64String(imgsrc));

        Sprite sprite = Sprite.Create(scrnShot, new Rect(0, 0, scrnShot.width, scrnShot.height), new Vector2(.5f, .5f));
        img.sprite = sprite; 

    }

    public void Capture()
    {
        StartCoroutine(takeScreenShot()); // screenshot of a particular UI Element.
    }
}