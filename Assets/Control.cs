using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(ToMain());
	}

    IEnumerator ToMain()
    {
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => Input.anyKeyDown);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
