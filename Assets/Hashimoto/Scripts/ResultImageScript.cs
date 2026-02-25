using UnityEngine;
using UnityEngine.UI;

public class ResultImageScript : MonoBehaviour
{
    [SerializeField] Sprite _image;
    Image sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(GameManager.instance.isChangeImage)
        //{
        //    sr.sprite = _image;
        //}
    }
}
