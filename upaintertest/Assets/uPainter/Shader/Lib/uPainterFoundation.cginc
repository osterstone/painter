#ifndef UPAINTER_FOUNDATION
#define UPAINTER_FOUNDATION

uniform int		    _InScriptMode;
uniform	int			_BrushType; //1-solid brush;2-texture brush;
uniform int			_BrushStatus; //0-zeros;1-cursor;2-point;3-line 1pts;4-line 2pts;5-line 3pts;6-line 4pts
uniform int			_OperationStatus; //0-begin;1-normal;2-end;
uniform int			_EnableGlobalUV;
uniform int			_EnableGlobalRepeatCount;
uniform float		_BrushSize;

uniform float		_Softness;
uniform float		_TotalLineDistance;
uniform int			_EnableOverlay;

float4 SampleTexture(sampler2D tex, float2 uv) {
#if SHADER_TARGET < 30
	return tex2D(tex, uv);
#else
	return tex2Dlod(tex, float4(uv, 0, 0));
#endif
}

bool ExistPointInTriangle(float3 p, float3 t1, float3 t2, float3 t3)
{
	const float TOLERANCE = 1 - 0.1;

	float3 a = normalize(cross(t1 - t3, p - t1));
	float3 b = normalize(cross(t2 - t1, p - t2));
	float3 c = normalize(cross(t3 - t2, p - t3));

	float d_ab = dot(a, b);
	float d_bc = dot(b, c);

	if (TOLERANCE < d_ab && TOLERANCE < d_bc) {
		return true;
	}
	return false;
}

float2 Rotate(float2 p, float rotate) {
	float rad = rotate;
	float cosr = 0, sinr = 0;
	sincos(rad, sinr, cosr);
	float x = p.x * cosr - p.y * sinr;
	float y = p.x * sinr + p.y * cosr;
	return float2(x, y);
}

float Point2LineDistanceSimple(float2 p, float2 p1, float2 p2) 
{
	float a = p1.y - p2.y;
	float b = p2.x - p1.x;
	return abs(a*p.x + b * p.y + p1.x*p2.y - p2.x*p1.y) / sqrt(a*a + b * b);
}

float2 GetBezierAt(float2 a, float2 b, float2 c, float2 d, float t)
{
	float omt = 1.0 - t;                                           
	float omt2 = omt * omt;                                    
	float omt3 = omt * omt2;                                   
	float t2 = t * t;                                          
	float t3 = t * t2;                                         
	return  omt3 * a +
			3.0 * t * omt2 * b +
			3.0 * t2 * omt * c +
			t3 * d;
}

float2 Point2LineProject(float2 p, float2 a, float2 b)
{
	float2 ba = b - a;
	float2 pa = p - a;

	float f = dot(ba, pa);
	if (f < 0)
		return a;

	float d = dot(ba, ba);
	if (f > d)
		return b;

	f = f / d;
	return a + f * ba;
}

float2 prjPos12, prjPos23;
float distX1, distX2;
void PreProcess(float2 mainUV, float4 uvs[4]) 
{
	prjPos12 = Point2LineProject(mainUV, uvs[1].xy, uvs[2].xy);
	prjPos23 = Point2LineProject(mainUV, uvs[2].xy, uvs[3].xy);

	distX1 = distance(uvs[1], prjPos12);
	distX2 = distance(uvs[2], prjPos12);
}

float CalcNeqWidthInLine(float2 mainUV, float4 p0, float4 p1)
{
	float w0_2 = p0.w * 0.5;
	float w1_2 = p1.w * 0.5;
	float2 prj = Point2LineProject(mainUV.xy, p0.xy, p1.xy);
	float ratio = distance(p0.xy, prj) / distance(p0.xy, p1.xy);
	return (w1_2 - w0_2) * ratio + w0_2;
}

float2 CalcBrushUV(float2 mainUV, float4 paintUV, float rotate) 
{
#if !UNITY_UV_STARTS_AT_TOP
	rotate += 3.1415926;
#endif

	float size = (1 - _EnableGlobalUV) * paintUV.w + _EnableGlobalUV * ((1 - _EnableGlobalRepeatCount) * _BrushSize + _EnableGlobalRepeatCount);;
	float2 pos = (1 - _EnableGlobalUV) * paintUV.xy;

	int offset = 1 - _EnableGlobalUV * _EnableGlobalRepeatCount;
#if UNITY_UV_STARTS_AT_TOP
	return Rotate((mainUV - pos) / size, -rotate) + float2(0.5,0.5) * offset;
#else
	return Rotate((pos - mainUV) / size, rotate) + float2(0.5, 0.5) * offset;
#endif
}

float2 CalcBrushUV2(float2 mainUV, float4 start, float4 end, float rotate) 
{
#if !UNITY_UV_STARTS_AT_TOP
	rotate += 3.1415926 * _EnableGlobalUV;
#endif

	float2 diff = end.xy - start.xy;

	float w = (1- _EnableGlobalUV) * start.w + _EnableGlobalUV * ((1 - _EnableGlobalRepeatCount) * _BrushSize + _EnableGlobalRepeatCount);
	float2 size = float2(w, w);
	float2 rot = atan2(diff.y, diff.x);
	float2 uv = mainUV;

#ifdef UPAINTER_NEQ_WIDTH_LINE
	size.y = CalcNeqWidthInLine(mainUV, start, end) * 2 * (1- _EnableGlobalUV) + _EnableGlobalUV*size.y;
#endif

	if (_EnableGlobalUV == 0)
	{
		float2 pos = start.xy;
#ifdef UPAINTER_CAP_ROUND
		float2 offset = float2(0.5, 0.5);
#else
		float2 offset = float2(0, 0.5);
#endif
		uv = Rotate((uv - pos) / size, -rot) + offset;
		uv.x = frac(uv.x + _TotalLineDistance);
		uv = frac(uv);
		uv = Rotate((uv - 0.5), -rotate) + 0.5;
	}
	else
	{
		int offset = 1 - _EnableGlobalUV * _EnableGlobalRepeatCount;
#if UNITY_UV_STARTS_AT_TOP
		uv = Rotate(mainUV / size, -rotate) + float2(0.5, 0.5) * offset;
#else
		uv = Rotate(-mainUV / size, rotate) + float2(0.5, 0.5) * offset;
#endif
	}

	return uv;
}

float IsPointInCircle(float2 mainUV, float4 pos)
{
	float dist = distance(mainUV, pos.xy);
	float r_2 = pos.w * 0.5;
	return  saturate(r_2 - dist) / r_2;
}

float IsPointInRect(float2 mainUV, float4 pos, float rotate)
{
	float w_2 = pos.w * 0.5;
	float3 p = float3(mainUV, 0);
	float3 v1 = float3(Rotate(float2(-w_2, w_2), rotate) + pos.xy, 0);
	float3 v2 = float3(Rotate(float2(-w_2, -w_2), rotate) + pos.xy, 0);
	float3 v3 = float3(Rotate(float2(w_2, -w_2), rotate) + pos.xy, 0);
	float3 v4 = float3(Rotate(float2(w_2, w_2), rotate) + pos.xy, 0);

	bool ret = ExistPointInTriangle(p, v1, v2, v3) + ExistPointInTriangle(p, v1, v3, v4);
	float dist = distance(mainUV, pos.xy);

	// sqrt(2)=1.414213
	w_2 = w_2 * 1.414213;

	return ret * saturate(w_2 - dist) / w_2;
}

float RecalculateByDistance(float2 mainUV, float4 p0, float4 p1, float ret)
{
	float w0_2 = p0.w * 0.5;
	float w1_2 = p1.w * 0.5;

	float dist = Point2LineDistanceSimple(mainUV, p0.xy, p1.xy);
	float width = w0_2;

#ifdef UPAINTER_NEQ_WIDTH_LINE
	float2 prj = Point2LineProject(mainUV, p0.xy, p1.xy);
	float ratio = distance(p0.xy, prj) / distance(p0.xy, p1.xy);
	width = (w1_2 - w0_2) * ratio + w0_2;
#endif

	return ret * saturate(width - dist) / width;
}

//float IsPointInFlatLine2(float2 mainUV, float4 p0, float4 p1) {
//	float2 size = p1 - p0;
//	float rotate = atan2(size.y, size.x);
//
//	float w0_2 = p0.w * 0.5;
//	float w1_2 = p1.w * 0.5;
//	float3 p = float3(mainUV, 0);
//	float3 v1 = float3(Rotate(float2(0, w0_2), rotate) + p0.xy, 0);
//	float3 v2 = float3(Rotate(float2(0, -w0_2), rotate) + p0.xy, 0);
//	float3 v3 = float3(Rotate(float2(0, -w1_2), rotate) + p1.xy, 0);
//	float3 v4 = float3(Rotate(float2(0, w1_2), rotate) + p1.xy, 0);
//
//	bool ret = (ExistPointInTriangle(p, v1, v2, v3) + ExistPointInTriangle(p, v1, v3, v4));
//
//	return RecalculateByDistance(mainUV, p0, p1, ret);
//}

float IsPointInFlatLine2(float2 mainUV, float4 p0, float4 p1) {
	float2 dir = normalize(p1 - p0);

	float2 leftDir = normalize(mainUV - p0);
	bool fromStart = dot(dir, leftDir) >= 0;
	float2 rightDir = normalize(mainUV - p1);
	bool toEnd = dot(-dir, rightDir) >= 0;

	float2 prj = Point2LineProject(mainUV, p0.xy, p1.xy);
	float dist = distance(mainUV, prj);

	float w0_2 = p0.w * 0.5;
	float width = w0_2;

#ifdef UPAINTER_NEQ_WIDTH_LINE
	float w1_2 = p1.w * 0.5;
	float ratio = distance(p0.xy, prj) / distance(p0.xy, p1.xy);
	width = (w1_2 - w0_2) * ratio + w0_2;
#endif

#ifdef UPAINTER_CAP_ROUND
	bool useStart = !fromStart && (_OperationStatus == 0);
	bool useEnd = !toEnd && (_OperationStatus == 2);
	bool inLine = ((_OperationStatus == 1 || _OperationStatus == 0) && (fromStart && toEnd));
	bool inCap = (useStart || useEnd);
	return (inLine || inCap) * (saturate(width - dist) / width);
#else
	return (fromStart && toEnd) * saturate(width - dist) / width;
#endif
}

float IsPointInRoundLine2(float2 mainUV, float4 p0, float4 p1)
{
	float2 prj = Point2LineProject(mainUV, p0.xy, p1.xy);
	float dist = distance(mainUV, prj);

	float w0_2 = p0.w * 0.5;
	float width = w0_2;

#ifdef UPAINTER_NEQ_WIDTH_LINE
	float w1_2 = p1.w * 0.5;
	float ratio = distance(p0.xy, prj) / distance(p0.xy, p1.xy);
	width = (w1_2 - w0_2) * ratio + w0_2;
#endif
	
	return saturate(width - dist) / width;
}

float4 GetPointsInCenter(float4 p0, float4 p1, float4 p2, out int dirsign)
{
	float2 v10 = normalize(p0.xy - p1.xy);
	float2 v12 = normalize(p2.xy - p1.xy);

	float sg = v10.x*v12.y - v12.x*v10.y;
	dirsign = step(sg, 0) * 2 - 1;

	float dotangle = dot(v10, v12);
	float angle = acos(dotangle);
	float4 p;

	bool sharp = false;
#if UPAINTER_CORNER_SHARP
	sharp = true;
#endif

	//parallel lines & flat mode
	if (!sharp || abs(dotangle-1) < 0.001 || abs(sg) < 0.001)
	{
		float2 size = p1 - p0;
		float rotate = atan2(size.y, size.x);
		float w1_2 = p1.w * 0.5;
		p.xy = Rotate(float2(0, w1_2), rotate) + p1.xy;
		p.zw = Rotate(float2(0, -w1_2), rotate) + p1.xy;
	}
	else
	{
		//sharp mode
		/*float dist = distance(p1, p2);
		float maxW = dist / cos(angle * 0.5);*/

		float w1_2 = (p1.w / sin(angle * 0.5)) * 0.5;
		w1_2 = max(p1.w * 0.5, w1_2);
		w1_2 = min(p1.w, w1_2);
		//w1_2 = min(maxW, w1_2);
		float2 cdir = dirsign * normalize(v10 + v12);

		p.xy = p1.xy + cdir * w1_2;
		p.zw = p1.xy - cdir * w1_2;
	}

	return p;
}

float FillFlatLine (float2 mainUV, float4 p0, float4 p1, float4 p2, float4 p3)
{
	if (_OperationStatus == 2 && _Softness > 0)
	{
		return IsPointInFlatLine2(mainUV, p1, p2);
	}

	float3 p = float3(mainUV, 0);
	float ret = 0;

	float preValue = IsPointInFlatLine2(mainUV, p0, p1);
	float curValue = IsPointInFlatLine2(mainUV, p1, p2);

	float2 diff01 = normalize(p1.xy - p0.xy);
	float2 diff12 = normalize(p2.xy - p1.xy);

	float dir = diff01.x * diff12.y - diff12.x * diff01.y;
	float2 mainDir = normalize(mainUV - p1.xy);
	float mdirCrs = diff01.x * mainDir.y - mainDir.x * diff01.y;

	float pSign = sign(preValue);
	float value = pSign * sign(mdirCrs * dir < 0) * (curValue - preValue) + (1 - pSign)*curValue;

	if (value == 0 && dir != 0 && (_OperationStatus != 0))
	{
		dir = -dir / abs(dir);
		//fill splited space
		float3 cv1 = cross(float3(0, 0, dir), float3(diff01.xy, 0));
		float3 cv2 = cross(-float3(diff12.xy, 0), float3(0, 0, dir));
		float3 v1 = float3(cv1.xy + p1.xy, 0);
		float3 v2 = float3(cv2.xy + p1.xy, 0);

#ifdef UPAINTER_CORNER_FLAT
		if (_EnableOverlay == 1)
		{
			return ExistPointInTriangle(p, float3(p1.xy, 0), v1, v2) * IsPointInCircle(mainUV, p1);
		}
		else
		{
			ret = max(ret, ExistPointInTriangle(p, float3(p1.xy, 0), v1, v2) * IsPointInCircle(mainUV, p1));
		}
#elif UPAINTER_CORNER_ROUND
		float2 npv1 = normalize(v1.xy - p1.xy);
		float2 npv2 = normalize(v2.xy - p1.xy);

		if (_EnableOverlay == 1)
		{
			float crs1 = dot(npv1, npv2);
			float crs2 = dot(npv1, mainDir);
			return (crs1 < crs2) * IsPointInCircle(mainUV, p1);
		}
		else
		{
			float3 c1 = cross(float3(mainDir.x, mainDir.y, 0), float3(npv1.x, npv1.y, 0));
			float3 c2 = cross(float3(mainDir.x, mainDir.y, 0), float3(npv2.x, npv2.y, 0));
			ret += (c1.z >= 0 && c2.z <= 0 && dir > 0 || c1.z <= 0 && c2.z >= 0 && dir < 0) * IsPointInCircle(mainUV, p1);
		}
#endif
	}
	
	ret += IsPointInFlatLine2(mainUV, p1, p2);	
	return ret;
}

float IsPointInRoundLine(float2 mainUV, float4 p0, float4 p1, float4 p2, float4 p3)
{
	//if (_Softness > 0 && _EnableOverlay == 1)
	//{
	//	// Only _Softness == 0 and line start will enter in this code
	//	// when paint lifetime not begin
	//	bool preValue = (_OperationStatus != 0) && (IsPointInRoundLine2(mainUV, p0, p1) > 0);
	//	if (preValue)
	//	{
	//		return 0;
	//	}
	//}

	/*float ret = 0;
	if (_Softness > 0)
	{
		 ret = IsPointInCircle(mainUV, p1);
	}
	return max(ret, IsPointInRoundLine2(mainUV, p1, p2));*/

	return IsPointInRoundLine2(mainUV, p1, p2);
}

float FillSharpLine(float2 mainUV, float4 p0, float4 p1, float4 p2, float4 p3)
{
	float3 p = float3(mainUV, 0);
	int dirsign12, dirsign34;
	float4 v12 = GetPointsInCenter(p0, p1, p2, dirsign12);
	float4 v34 = GetPointsInCenter(p1, p2, p3, dirsign34);

	float3 v1 = float3(v12.xy, 0);
	float3 v2 = float3(v12.zw, 0);
	float3 v3 = float3(v34.zw, 0);
	float3 v4 = float3(v34.xy, 0);

	bool ret = max(ExistPointInTriangle(p, v1, v2, v3), ExistPointInTriangle(p, v1, v3, v4)) * (_OperationStatus != 0) > 0;

	return RecalculateByDistance(mainUV, p1, p2, ret);
}

float IsPointInLineEx(float2 mainUV, float4 p0, float4 p1, float4 p2, float4 p3) 
{
	float ret = 0;

#if UPAINTER_CAP_ROUND
	if (_Softness == 0 || _EnableOverlay == 0)
	{
		ret += IsPointInRoundLine(mainUV, p0, p1, p2, p3);
	}
	else
	{
		ret += FillFlatLine(mainUV, p0, p1, p2, p3);
	}
#else
	#if UPAINTER_CORNER_SHARP
		ret += FillSharpLine(mainUV, p0, p1, p2, p3);
	#else
		ret += FillFlatLine(mainUV, p0, p1, p2, p3);
	#endif
#endif

	return ret;
}

float IsPointInCurveLine2(float2 mainUV, float4 p0, float4 p1, float4 p2, float4 p3, float4 ctrl0, float4 ctrl1, float4 ctrl2, float4 ctrl3) {
	float dist = 0;
	float4 pre0 = p1;
	float4 pre1 = p1;
	float4 next0 = p1;

	float2 prepos0 = GetBezierAt(p0.xy, ctrl0.zw, ctrl1.xy, p1.xy, 0.6);
	float2 prepos1 = GetBezierAt(p0.xy, ctrl0.zw, ctrl1.xy, p1.xy, 0.8);
	float2 nextpos0 = GetBezierAt(p1.xy, ctrl1.zw, ctrl2.xy, p2.xy, 0.2);
	pre0 = float4(prepos0.x, prepos0.y, p0.z, p0.w);
	pre1 = float4(prepos1.x, prepos1.y, p0.z, p0.w);
	next0 = float4(nextpos0.x, nextpos0.y, p0.z, p0.w);

#ifdef UPAINTER_NEQ_WIDTH_LINE
	pre0.w = lerp(p0.w, p1.w, 0.6);
	pre1.w = lerp(p0.w, p1.w, 0.8);
	next0.w = lerp(p1.w, p2.w, 0.2);
#endif

#if UPAINTER_CORNER_SHARP
	dist = IsPointInLineEx(mainUV, pre0, pre1, p1, next0);
#endif

	pre0 = pre1;
	pre1 = p1;
	int count = 5;
	int oldStatus = _OperationStatus;
	
	for (int i = 0; i < count; i++) {
		float s = i * 0.2;
		float e = s + 0.2;
		float e1 = s + 0.4;
		float2 pos1 = pre1.xy;
		float2 pos2 = GetBezierAt(p1.xy, ctrl1.zw, ctrl2.xy, p2.xy, e);
		float2 pos3 = GetBezierAt(p1.xy, ctrl1.zw, ctrl2.xy, p2.xy, e1);

		float dw = p2.w - p1.w;
		float sw = p1.w + dw * s;
		float ew = p1.w + dw * e;
		float ew1 = p1.w + dw * e1;

		next0 = float4(pos3.x, pos3.y, 0, ew1);
		int islast = step(1 - count + 1, 0);
		next0 = next0 * (1 - islast) + p2 * islast;

		float4 epos1 = float4(pos1.x, pos1.y, p1.z, sw);
		float4 epos2 = float4(pos2.x, pos2.y, p2.z, ew);

		if (oldStatus != 2 || _Softness == 0)
		{
			dist = max(dist, IsPointInLineEx(mainUV, pre0, epos1, epos2, next0));
		}
		else if (oldStatus == 2 && i == count - 1)
		{
			_OperationStatus = oldStatus;
			dist = max(dist, IsPointInLineEx(mainUV, pre0, epos1, epos2, next0));
		}

		pre0 = epos1;
		pre1 = epos2;

		// debug
		//dist += IsPointInCircle(mainUV, float4(ctrl0.z, ctrl0.w, 0, 0.005));// +IsPointInCircle(mainUV, float4(ctrl1.x, ctrl1.y, 0, 0.005));
		_OperationStatus = 1;
	}
	_OperationStatus = oldStatus;

	return dist;
}

float IsPointInLine3(float2 mainUV, float4 uvs[4], float4 controls[4])
{
#if UPAINTER_CAP_ROUND
	if (0 == _EnableOverlay && _OperationStatus == 2) {
		return 0;
	}
#else
	if (_OperationStatus == 2) {
		return 0;
	}
#endif

	float4 p0 = uvs[0];
	float4 p1 = uvs[1]; 
	float4 p2 = uvs[2];
	float4 p3 = uvs[3];

	float ret = 0;
#ifdef UPAINTER_ENABLE_BEZIER
	ret += IsPointInCurveLine2(mainUV,p0, p1, p2, p3, controls[0], controls[1], controls[2], controls[3]);
#else
	ret += IsPointInLineEx(mainUV, p0, p1, p2, p3);
#endif	

	return ret;
}

#ifdef UPAINTER_POINT_MODE
	#ifdef UPAINTER_CAP_FLAT
		#define UPAINTER_IN_PAINT_RANGE(mainUV, uvs, rotate, controls) IsPointInRect(mainUV, uvs[0], rotate)
	#elif UPAINTER_CAP_ROUND
		#define UPAINTER_IN_PAINT_RANGE(mainUV, uvs, rotate, controls) IsPointInCircle(mainUV, uvs[0])
	#endif
#elif UPAINTER_LINE_MODE
	#define UPAINTER_IN_PAINT_RANGE(mainUV, uvs, rotate, controls) IsPointInLine3(mainUV, uvs, controls)
#endif

#ifdef UPAINTER_POINT_MODE
	#define UPAINTER_CALC_BRUSH_UV(mainUV, uvs, rotate) CalcBrushUV(mainUV, uvs[0], rotate)
#else
	#define UPAINTER_CALC_BRUSH_UV(mainUV, uvs, rotate) CalcBrushUV2(mainUV, uvs[1], uvs[2], rotate)
#endif

#endif //UPAINTER_FOUNDATION