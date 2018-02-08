using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testbutton : MonoBehaviour {
    public Button test_button;
    public Text test_text;
    // Use this for initialization
    void Start () {
        Button btn = test_button.GetComponent<Button>();
        btn.onClick.AddListener(TestOnClick);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void TestOnClick ()
    {
        if(test_text.text == "" || test_text.text == "on")
        {
            test_text.text = "off";
        }
        else
        {
            test_text.text = "on";
        }
    }
}
