using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Es.InkPainter;

public class ColorSelector : MonoBehaviour {
	public Camera refCamera;
	public GameObject selectorImage,outerCursor,innerCursor;
    public RectTransform selectorRect;
	public Image finalColorSprite;

	Color finalColor, selectedColor;
	float selectorAngle=0.0f;
	Vector2 innerDelta=Vector2.zero;
	static ColorSelector myslf;

    int status = 0;

	void Awake () {
		myslf = this;
	}
	void Start () {
		if (refCamera == null)
			refCamera = Camera.main;
		selectedColor = Color.red;
        SelectOuterColor(new Vector2(0f, -1f));
		//SelectInnerColor (Vector2.zero);
		finalColorSprite.color=finalColor;

	}

	void Update () {
        if (Input.GetMouseButton(0))
        {
            UserInputUpdate();
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            status = 0;
        }
	}

    void UserInputUpdate(){
        var pos = selectorRect.InverseTransformPoint(Input.mousePosition);
        var x = pos.x;
        var y = pos.y;
        var w_2 = selectorRect.rect.width * 0.5f;
        var h_2 = selectorRect.rect.height * 0.5f;
        pos.x /= selectorRect.rect.width;
        pos.y /= selectorRect.rect.height;
        if (x >= -w_2 && y >= -h_2 && x <= w_2 && y <= h_2)
        {
            var d = new Vector2(0.4f, 0.0f).sqrMagnitude;
            var dist = pos.sqrMagnitude;
            if (status == 0)
            {
                if (dist > 0.16f)
                {
                    SelectOuterColor(pos);
                    status = 1;
                }
                else
                {
                    SelectInnerColor(pos);
                    status = 2;
                }
            }
            else
            {
                if(status == 1)
                {
                    SelectOuterColor(pos);
                }
                else
                {
                    SelectInnerColor(pos);
                }
            }
        }
    }

    Vector2 getPos(Vector2 delta)
    {
        Vector2 size = new Vector2(selectorRect.rect.width * delta.x, selectorRect.rect.height * delta.y);
        return size;
    }

	void SelectInnerColor(Vector2 delta){
        delta = ClampTriangle(delta);

		float v=0.0f, w=0.0f, u=0.0f;
		Barycentric (delta,ref v,ref w,ref u);
        if (v >= 0.15f && w >= -0.15f && u >= -0.15f)
        {
			Vector3 colorVector = new Vector3 (selectedColor.r, selectedColor.g, selectedColor.b);
			Vector3 finalColorVector = v * colorVector + u * new Vector3 (0.0f, 0.0f, 0.0f) + w * new Vector3 (1.0f, 1.0f, 1.0f);
			finalColor = new Color (finalColorVector.x, finalColorVector.y, finalColorVector.z);

			finalColorSprite.color=finalColor;

            innerCursor.GetComponent<RectTransform>().anchoredPosition = getPos(delta);
			innerDelta = delta;
		}

	}
	Vector3 ClampPosToCircle(Vector3 pos){
		Vector3 newPos = Vector3.zero;
        float dist = 0.9f; // 0.225f;
		float angle = Mathf.Atan2(pos.x, pos.y);// * 180 / Mathf.PI;

		newPos.x = dist * Mathf.Sin( angle ) * selectorRect.rect.width * 0.5f;
        newPos.y = dist * Mathf.Cos(angle) * selectorRect.rect.height * 0.5f;

        newPos.z = pos.z;
		return newPos;
	}

    Vector2 ClampTriangle(Vector2 point)
    {
        Vector2 a = new Vector2(0.0f, 0.4f);
        Vector2 b = new Vector2(-0.355f, -0.183f);
        Vector2 c = new Vector2(0.355f, -0.183f);

        Vector2 result = Vector2.zero;

        if(Math.GetIntersection(a, b, Vector2.zero, point, ref result) ||
            Math.GetIntersection(b, c, Vector2.zero, point, ref result) ||
            Math.GetIntersection(c, a, Vector2.zero, point, ref result))
        {
            return result;
        }

        return point;
    }

	void Barycentric(Vector2 point,ref float u,ref float v,ref float w){

		Vector2 a = new Vector2 (0.0f, 0.27f);
		Vector2 b = new Vector2 (-0.32f, -0.3f);
		Vector2 c = new Vector2 (0.32f, -0.3f);

		Vector2 v0 = b - a, v1 = c - a, v2 = point - a;
		float d00 = Vector2.Dot(v0, v0);
		float d01 = Vector2.Dot(v0, v1);
		float d11 = Vector2.Dot(v1, v1);
		float d20 = Vector2.Dot(v2, v0);
		float d21 = Vector2.Dot(v2, v1);
		float denom = d00 * d11 - d01 * d01;
		v = (d11 * d20 - d01 * d21) / denom;
		w = (d00 * d21 - d01 * d20) / denom;
		u = 1.0f - v - w;
	}


	void SelectOuterColor(Vector2 delta){
		float angle= Mathf.Atan2(delta.x, delta.y);
		float angleGrad=angle*57.2957795f;
		if(angleGrad<0.0f)
			angleGrad=360+angleGrad;
		selectorAngle=angleGrad/360;
		selectedColor=HSVToRGB(selectorAngle,1.0f,1.0f);
		selectorImage.GetComponent<RawImage>().material.SetColor("_Color",selectedColor);
        outerCursor.GetComponent<RectTransform>().anchoredPosition = ClampPosToCircle (getPos(delta));/// delta*0.75f;
		SelectInnerColor (innerDelta);

	}
	public static Color HSVToRGB(float H, float S, float V)
	{
		if (S == 0f)
			return new Color(V,V,V);
		else if (V == 0f)
			return Color.black;
		else
		{
			Color col = Color.black;
			float Hval = H * 6f;
			int sel = Mathf.FloorToInt(Hval);
			float mod = Hval - sel;
			float v1 = V * (1f - S);
			float v2 = V * (1f - S * mod);
			float v3 = V * (1f - S * (1f - mod));
			switch (sel + 1)
			{
			case 0:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 1:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			case 2:
				col.r = v2;
				col.g = V;
				col.b = v1;
				break;
			case 3:
				col.r = v1;
				col.g = V;
				col.b = v3;
				break;
			case 4:
				col.r = v1;
				col.g = v2;
				col.b = V;
				break;
			case 5:
				col.r = v3;
				col.g = v1;
				col.b = V;
				break;
			case 6:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 7:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			}
			col.r = Mathf.Clamp(col.r, 0f, 1f);
			col.g = Mathf.Clamp(col.g, 0f, 1f);
			col.b = Mathf.Clamp(col.b, 0f, 1f);
			return col;
		}
	}

	public static Color GetColor()
    {
		return myslf.finalColor;
	}

    public static void SetColor(Color color)
    {
        myslf.finalColor = color;
        myslf.finalColorSprite.color = color;
    }
}