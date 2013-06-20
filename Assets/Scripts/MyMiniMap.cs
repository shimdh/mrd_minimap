using UnityEngine;
using System.Collections;

#region FileDescription
// 챕터맵
// 유니티상의 좌표(0, Y, 0) -> 픽셀 좌표(451, 883), 픽셀좌표 기준(왼쪽, 위)가 (0, 0)
// 픽셀좌표 기준을 변경 (왼쪽, 아래)가 (0, 0)
// (451, 883) -> (451, 923 - 883) -> (451, 40)
// 이미지상의 픽셀 비교할 픽셀 좌표들 : (451, 883), (418, 148)
// 유니티상의 비교 좌표들(0, Y, 0), (-36.2, Y, 834.6)
// 픽셀상의 차이 : 유니티상의 차이 = X : 1
// 33 : 36.2 = X : 1
// X = 33 / 36.2 => 1유니티상의 픽셀거리
// 첫번째 챕터맵의 좌표
// 원래 픽셀상의 좌표(294, 601)
// 기준좌표계를 아래 왼쪽으로 바꿈 (294, 923 - 601) -> (294, 322)
// (294, 322)
// 화면에 보이는 비율로 변환한 좌표
// (294 * ratio, 322, * ratio) -> (117.6, 128.8)
#endregion
public class MyMiniMap : MonoBehaviour
{
	public enum ChapterMapTypes
	{ // 챕터맵의 종류
		chapter_1_1_1
,
		chapter_1_1_2
,
		chapter_1_1_3
,
		chapter_1_2_1
,
		chapter_1_2_2
,
		chapter_1_2_3
,
		chapter_1_3_1
,
		chapter_1_3_2
,
		chapter_1_3_3
,
		chapter_1_4_1
,
		chapter_1_4_2
,
		chapter_1_4_3
,
	};

	private int[] totalmapWidthPx = {923, 923, 923, 923};// 전체 맵의 픽셀 너비 923 픽셀
	private int[] totalmapHeightPx = {923, 923, 923, 923};// 전체 맵의 픽셀 높이 923 픽셀
	private int[] chaptermapWidth = {314, 314, 314, 314};// 챕터 맵의 픽셀 너비 314 픽셀
	private int[] chaptermapHeight = {314, 314, 314, 314};// 챕터 맵의 픽셀 높이 314 픽셀

	private int[] pixelZeroXPx = {451, 451, 451, 451};// 전체이미지맵에서의 픽셀 X좌표
	private int[] pixelZeroYPx = {883, 883, 883, 883};// 전체이미지맵에서의 픽셀 Y좌표

	private double unitPerPixelRatio;// 1유니티상의 픽셀거리
	private double[] unitPerPixelRatioArray = {33 / 36.2,
		0.09374527507073417,
		0.2917690171986541,
		0.7905554251593446,
	};
	private Vector2[] chapterMapPos;// 챕터맵별로 적용되는 위치값배열

	private ChapterMapTypes _chapterMapType = ChapterMapTypes.chapter_1_1_1;
	public ChapterMapTypes chapterMapType {
		get{ return this._chapterMapType;}
		set{
			this._chapterMapType = value;
			this.SetupPositions ();
			this.SetGuiVariables ();
			this.ApplyChapterPositionPixel();
		}
	}

	public GameObject PlayerDotGUI;// 플레이어의 닷 GUI 게임오브젝트
	public GameObject Player;// 플레이어 게임오브젝트

	public GameObject DotEnemies;// 몬스터 점들을 관리하는 부모오브젝트
	public GameObject DotEnemyPrefab;// 몬스터 점의 프리팹

	public Texture[] ChapterMaps;// 챕터맵들의 텍스쳐배열

	private Vector3 PlayerPosition;// 플레이어의 좌표값을 변수에 대입
	// 픽셀의 비율(전체 원본픽셀을 표시하면 화면에 거의 꽉 참, 화면에 비율에 맞춰 축소)
	public float RatioScale = 0.4f;
	public float[] RatioScales = {0.4f, 0.4f, 0.4f, 0.4f};

	private int chaptermapPosXPx;// 챕터맵의 왼쪽 아래부분의 X좌표(0,0)을 왼쪽 아래로 기준으로 함
	private int chaptermapPosYPx;// 챕터맵의 왼쪽 아래부분의 Y좌표(0,0)을 왼쪽 아래로 기준으로 함

	private double pixelWidth;// 원본 픽셀에 픽셀 비율을 적용한 픽셀 너비
	private double pixelHeight;// 원본 픽셀에 픽셀 비율을 적용한 픽셀 높이
	private double pixelX;// 적용된 픽셀의 너비를 바탕으로 한 픽셀의 X 좌표
	private double pixelY;// 적용된 픽셀의 높이를 바탕으로 한 픽셀의 Y 좌표
	private double guiWidth;// 0과 1사이의 GUI좌표값으로의 텍스쳐의 너비(GUI상의 너비)
	private double guiHeight;// 0과 1사이의 GUI좌표값으로의 텍스처의 높이(GUI상의 높이)
	private double gui_bottom_left_x;// 0과 1사이의 GUI좌표값으로의 텍스쳐의 아래왼쪽의 X좌표(GUI상의 X좌표)
	private double gui_bottom_left_y;// 0과 1사이의 GUI좌표값으로의 텍스쳐의 아래왼쪽의 Y좌표(GUI상의 Y좌표)
	private double converted_totalmap_player_pos_x_px;// 유니티상의 좌표를 전체맵상에서의 이미지좌표에서 (0,0)을 왼쪽아래를 기준으로 바꾼 X좌표
	private double converted_totalmap_player_pos_y_px;// 유니티상의 좌표를 전체맵상에서의 이미지좌표에서 (0,0)을 왼쪽아래를 기준으로 바꾼 y좌표
	private double converted_chaptermap_player_pos_x_px;// converted_totalmap_player_pos_x_px 를 챕터맵을 기준좌표로 바꾼 X좌표
	private double converted_chaptermap_player_pos_y_px;// converted_totalmap_player_pos_y_px 를 챕터맵을 기준좌표로 바꾼 Y좌표
	private double gui_chaptermap_player_pos_x_scale;//gui상의 0과 1사이의 좌표계로 변환한 X좌표(scale 적용, gui상의 챕터맵의 왼쪽 오른쪽 좌표도 적용)
	private double gui_chaptermap_player_pos_y_scale;//gui상의 0과 1사이의 좌표계로 변환한 Y좌표(scale 적용, gui상의 챕터맵의 왼쪽 오른쪽 좌표도 적용)

	public float refeshTimeDisplayingDotEnemies = 2;

	// 미니맵을 화면에 보여주는 상태를 위한 변수들
	private float currentAddTimeDisplayingDotEnemies = 0;
	private bool canShow = true;

	private int currentPixelZeroX_px = 0;
	private int currentPixelZeroY_px = 0;
	private int currentTotalmapHeight_px = 0;

	private void SetupCoodination(int index) {
		RatioScale = RatioScales [index];
		unitPerPixelRatio = unitPerPixelRatioArray[index];
		currentPixelZeroX_px = pixelZeroXPx[index];
		currentPixelZeroY_px = pixelZeroYPx[index];
		currentTotalmapHeight_px = totalmapHeightPx[index];
	}

	private void SetupPositions() {
		switch (chapterMapType) {
		case ChapterMapTypes.chapter_1_1_1:
		case ChapterMapTypes.chapter_1_1_2:
		case ChapterMapTypes.chapter_1_1_3:
			SetupCoodination(0);
			break;

		case ChapterMapTypes.chapter_1_2_1:
		case ChapterMapTypes.chapter_1_2_2:
		case ChapterMapTypes.chapter_1_2_3:
			SetupCoodination(1);
			break;

		case ChapterMapTypes.chapter_1_3_1:
		case ChapterMapTypes.chapter_1_3_2:
		case ChapterMapTypes.chapter_1_3_3:
			SetupCoodination(2);
			break;

		case ChapterMapTypes.chapter_1_4_1:
		case ChapterMapTypes.chapter_1_4_2:
		case ChapterMapTypes.chapter_1_4_3:
			SetupCoodination(3);
		break;

		default:
			break;
		}
	}

	void SetGuiVariables ()
	{
		pixelWidth = transform.guiTexture.texture.width * RatioScale;
		pixelHeight = transform.guiTexture.texture.height * RatioScale;
		pixelX = -(pixelWidth / 2);
		pixelY = -(pixelHeight / 2);
		//적용시킨 픽셀인셋
		transform.guiTexture.pixelInset = new Rect ((float)pixelX, (float)pixelY, (float)pixelWidth, (float)pixelHeight);
		guiWidth = (transform.guiTexture.texture.width * RatioScale) / Screen.width;
		guiHeight = (transform.guiTexture.texture.height * RatioScale) / Screen.height;
		gui_bottom_left_x = transform.position.x - (guiWidth / 2);
		gui_bottom_left_y = transform.position.y - (guiHeight / 2);
	}

	void Awake() {
		chapterMapPos = new []{
			new Vector2(294f, (float)(totalmapHeightPx[0] - chaptermapHeight[0] - 601)),
			new Vector2(192f, (float)(totalmapHeightPx[0] - chaptermapHeight[0] - 300)),
			new Vector2(369f, (float)(totalmapHeightPx[0] - chaptermapHeight[0] - 0)),
			new Vector2(501f, (float)(totalmapHeightPx[1] - chaptermapHeight[1] - 419)),
			new Vector2(345f, (float)(totalmapHeightPx[1] - chaptermapHeight[1] - 188)),
			new Vector2(91f, (float)(totalmapHeightPx[1] - chaptermapHeight[1] - 188)),
			new Vector2(219f, (float)(totalmapHeightPx[2] - chaptermapHeight[2] - 91)),
			new Vector2(331f, (float)(totalmapHeightPx[2] - chaptermapHeight[2] - 295)),
			new Vector2(374f, (float)(totalmapHeightPx[2] - chaptermapHeight[2] - 493)),
			new Vector2(400f, (float)(totalmapHeightPx[3] - chaptermapHeight[3] - 112)),
			new Vector2(217f, (float)(totalmapHeightPx[3] - chaptermapHeight[3] - 331)),
			new Vector2(107f, (float)(totalmapHeightPx[3] - chaptermapHeight[3] - 525)),
		};// 챕터맵별로 적용되는 위치값배열
		unitPerPixelRatio = 33 / 36.2;

		this.chapterMapType = ChapterMapTypes.chapter_1_1_1;
//		SetGuiVariables ();
	}

	// Use this for initialization
	IEnumerator Start () {
		yield return StartCoroutine(AssignPlayerObject());
	}

	IEnumerator AssignPlayerObject () {
		while(Player == null) {
			Player = GameObject.FindGameObjectWithTag("Player");
			yield return null;
		}

		if(Player != null) {
			ApplyChapterPositionPixel();
			CalculatePlayerPosition();
			DisplayDotEnemies();
			yield break;
		}
	}

	// Update is called once per frame
	void Update () {
		if (!canShow) {
			return;
		}

		currentAddTimeDisplayingDotEnemies += Time.deltaTime;
		if(currentAddTimeDisplayingDotEnemies > refeshTimeDisplayingDotEnemies) {
			CalculatePlayerPosition();
			DisplayDotEnemies();
			currentAddTimeDisplayingDotEnemies = 0;
		}

	}

	void OnGUI() {

	}

	/// <summary>
	/// 변경된 챕터맵에 따라 챕터맵의 텍스쳐와 좌표값들을 변경한다.
	/// 이 메소드를 호출하기전에 chapterMapType의 값을 원하는 챕터의 enum값으로 변경해야한다.
	/// </summary>
	public void ApplyChapterPositionPixel ()
	{
		chaptermapPosXPx = (int)chapterMapPos[(int)chapterMapType].x;
		chaptermapPosYPx = (int)chapterMapPos[(int)chapterMapType].y;
		guiTexture.texture = ChapterMaps[(int)chapterMapType];
	}

	/// <summary>
	/// 미니맵을 화면에 보이게 하거나 숨기기
	/// </summary>
	/// <param name='canControl'>
	/// 파라미터 값에 따라 화면에 보여주기를 판단한다.
	/// </param>
	public void EnableMiniMap(bool canControl) {
		gameObject.SetActiveRecursively(canControl);
		canShow = canControl;

		if(!canControl) {
			DestoryAllDotEnemy();
			PlayerDotGUI.SetActiveRecursively(false);
		}
		else {
			DisplayDotEnemies();
			PlayerDotGUI.SetActiveRecursively(true);
		}
	}

	/// <summary>
	/// 현재 미니맵이 화면에 나타나고 있는지 여부를 알려준다
	/// </summary>
	/// <returns>
	/// 미니맵의 상태를 돌려준다.(화면에 보여주고 있는지, 아닌지)
	/// </returns>
	public bool GetShowState() {
		return canShow;
	}

	void CalculatePlayerPosition ()
	{
		PlayerPosition = new Vector3(Player.transform.position.x,
		Player.transform.position.y, Player.transform.position.z);

//		SetupPositions();

		converted_totalmap_player_pos_x_px =
			PlayerPosition.x * unitPerPixelRatio + currentPixelZeroX_px;
		converted_totalmap_player_pos_y_px =
			PlayerPosition.z * unitPerPixelRatio + currentTotalmapHeight_px - currentPixelZeroY_px;
		converted_chaptermap_player_pos_x_px =
			converted_totalmap_player_pos_x_px - chaptermapPosXPx;
		converted_chaptermap_player_pos_y_px =
			converted_totalmap_player_pos_y_px - chaptermapPosYPx;
		gui_chaptermap_player_pos_x_scale =
			converted_chaptermap_player_pos_x_px * RatioScale / Screen.width + gui_bottom_left_x;
		gui_chaptermap_player_pos_y_scale =
			converted_chaptermap_player_pos_y_px * RatioScale / Screen.height + gui_bottom_left_y;

		PlayerDotGUI.transform.position = new Vector3(
			(float)gui_chaptermap_player_pos_x_scale,
			(float)gui_chaptermap_player_pos_y_scale, 1f);
	}

	void DisplayDotEnemies() {
		DestoryAllDotEnemy();

		GameObject[] enemies;
		enemies = GameObject.FindGameObjectsWithTag("Enemy");

		if (enemies.Length == 0) return;

		foreach(GameObject go in enemies) {
			Vector2 cal_enemy_pos = CalculateEnemyPosition(go.transform.position);
			if(cal_enemy_pos.x < gui_bottom_left_x
				|| cal_enemy_pos.y < gui_bottom_left_y
				|| cal_enemy_pos.x > gui_bottom_left_x + guiWidth
				|| cal_enemy_pos.y > gui_bottom_left_y + guiHeight) {
				continue;
			}
			GameObject dot_enemy_prefab = Instantiate(DotEnemyPrefab) as GameObject;
			dot_enemy_prefab.transform.position = new Vector3(
				cal_enemy_pos.x, cal_enemy_pos.y, 1f);
			dot_enemy_prefab.transform.parent = DotEnemies.transform;
		}
	}

	Vector2 CalculateEnemyPosition (Vector3 pos) {
//		SetupPositions();
		double converted_totalmap_enemy_pos_x_px =
			pos.x * unitPerPixelRatio + currentPixelZeroX_px;
		double converted_totalmap_enemy_pos_y_px =
			pos.z * unitPerPixelRatio + currentTotalmapHeight_px - currentPixelZeroY_px;
		double converted_chaptermap_enemy_pos_x_px =
			converted_totalmap_enemy_pos_x_px - chaptermapPosXPx;
		double converted_chaptermap_enemy_pos_y_px =
			converted_totalmap_enemy_pos_y_px - chaptermapPosYPx;
		double gui_chaptermap_enemy_pos_x_scale =
			converted_chaptermap_enemy_pos_x_px * RatioScale / Screen.width + gui_bottom_left_x;
		double gui_chaptermap_enemy_pos_y_scale =
			converted_chaptermap_enemy_pos_y_px * RatioScale / Screen.height + gui_bottom_left_y;

		return new Vector2(
			(float)gui_chaptermap_enemy_pos_x_scale,
			(float)gui_chaptermap_enemy_pos_y_scale);
	}


	/// <summary>
	/// 맵에 보이는 전체 몬스터의 점들을 없앤다.
	/// </summary>
	void DestoryAllDotEnemy() {

		foreach(Transform child in DotEnemies.transform) {
			Destroy(child.gameObject);
		}
	}
}
