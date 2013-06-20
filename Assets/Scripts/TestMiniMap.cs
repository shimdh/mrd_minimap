using UnityEngine;
using System.Collections;

public class TestMiniMap : MonoBehaviour
{
	private MyMiniMap miniMap;
	// Use this for initialization
	void Start ()
	{
		miniMap = GameObject.Find ("MapGUI").GetComponent<MyMiniMap> ();
	}
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyUp ("t")) {
			miniMap.chapterMapType = MyMiniMap.ChapterMapTypes.chapter_1_2_2;
			Debug.Log ("GetShowState() : " + miniMap.GetShowState());
			miniMap.EnableMiniMap (!miniMap.GetShowState());
		}
	}
}
