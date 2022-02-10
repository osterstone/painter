#ifndef UPAINTER_BLEND
#define UPAINTER_BLEND

#include "./noise.cginc"

float4 BlendNormal(float4 v1, float4 v2)
{
	return v1 * (1 - v2.a) + v2 * v2.a;
}

float4 BlendByAlpha(float4 v1, float4 v2, float alpha)
{
	return v1 * (1 - alpha) + v2 * alpha;
}

float4 BlendRestore(float4 v1, float v2)
{
	return v1;
}

float4 BlendReplace(float4 v1, float4 v2)
{
	return v2;
}

float4 BlendReplaceEx(float4 v1, float4 v2)
{
	float a = step(0.004, v2.a);
	return v1*(1-a) + v2*a;
}

/**************************Dark Mode****************************/
//变暗
float4 BlendDarken(float4 v1, float4 v2)
{
	return min(v1, v2);
}

//正片叠底
float4 BlendMutipy(float4 v1, float4 v2)
{
	return v1 * v2;
}

//颜色加深
float4 BlendColorBurn(float4 v1, float4 v2)
{
	return saturate(v1 - ((1 - v1) * (1 - v2)) / v2);
}

//线性加深
float4 BlendLinearDark(float4 v1, float4 v2)
{
	return v1 + v2 - 1;
}

/**************************Light Mode****************************/
//变亮
float4 BlendLighten(float4 v1, float4 v2)
{
	return max(v1, v2);
}

//滤色
float4 BlendColorScreen(float4 v1, float4 v2)
{
	return 1 - (1 - v1) * (1 - v2);
}

//颜色减淡
float4 BlendColorDodge(float4 v1, float4 v2)
{
	return v1 + saturate(v1 * v2 / (1 - v2));
}

//线性减淡
float4 BlendLinearDodge(float4 v1, float4 v2)
{
	return v1 + v2;
}

/**************************Saturation Mode****************************/
float blendOverlayMode(float v1, float v2)
{
	if (v1 <= 0.5)
	{
		return v1 * v2 * 0.5;
	}
	else
	{
		return 1 - (1 - v1)*(1 - v2)*0.5;
	}
}

//叠加
float4 BlendOverlay(float4 v1, float4 v2)
{
	float4 ret = float4(0,0,0,1);
	ret.x = blendOverlayMode(v1.x, v2.x);
	ret.y = blendOverlayMode(v1.y, v2.y);
	ret.z = blendOverlayMode(v1.z, v2.z);
	ret.a = blendOverlayMode(v1.a, v2.a);
	return ret;
}

//强光
float4 BlendHardLight(float4 v1, float4 v2)
{
	float4 ret = float4(0, 0, 0, 1);
	ret.x = blendOverlayMode(v2.x, v1.x);
	ret.y = blendOverlayMode(v2.y, v1.y);
	ret.z = blendOverlayMode(v2.z, v1.z);
	ret.a = blendOverlayMode(v2.a, v1.a);
	return ret;
}

//柔光
float softLightMode(float v1, float v2)
{
	if (v2 <= 0.5)
	{
		return v1 * v2 * 0.5 + pow(v1, 2)*(1 - 2 * v2);
	}
	else
	{
		return v1 * (1 - v2) * 0.5 + sqrt(v1) * (2 * v2 - 1);
	}
}
float4 BlendSoftLight(float4 v1, float4 v2)
{
	float4 ret = float4(0, 0, 0, 1);
	ret.x = softLightMode(v1.x, v2.x);
	ret.y = softLightMode(v1.y, v2.y);
	ret.z = softLightMode(v1.z, v2.z);
	ret.a = softLightMode(v1.a, v2.a);
	return ret;
}

//亮光
float vividLightMode(float v1, float v2)
{
	if (v2 <= 0.5)
	{
		return v1 - saturate((1 - v1)*(1 - 2 * v2) * 0.5 / v2);
	}
	else
	{
		return v1 + saturate(v1 * (v2 * 2 - 1)*0.5 / (1 - v2));
	}
}
float4 BlendVividLight(float4 v1, float4 v2)
{
	float4 ret = float4(0, 0, 0, 1);
	ret.x = vividLightMode(v1.x, v2.x);
	ret.y = vividLightMode(v1.y, v2.y);
	ret.z = vividLightMode(v1.z, v2.z);
	ret.a = vividLightMode(v1.a, v2.a);
	return ret;
}

//点光
float pinLightMode(float v1, float v2)
{
	if (v2 <= 0.5)
	{
		return min(v1, 2 * v2);
	}
	else
	{
		return min(v1, 2 * v2 - 1);
	}
}
float4 BlendPinLight(float4 v1, float4 v2)
{
	float4 ret = float4(0, 0, 0, 1);
	ret.x = pinLightMode(v1.x, v2.x);
	ret.y = pinLightMode(v1.y, v2.y);
	ret.z = pinLightMode(v1.z, v2.z);
	ret.a = pinLightMode(v1.a, v2.a);
	return ret;
}

//线形光
float4 BlendLinearLight(float4 v1, float4 v2)
{
	return v1 + v2 * 2 - 1;
}

//实色混合
float hardMixMode(float v1, float v2)
{
	return step(1, v1+v2);
}

float4 BlendHardMix(float4 v1, float4 v2)
{
	float4 ret = float4(0, 0, 0, 1);
	ret.x = hardMixMode(v1.x, v2.x);
	ret.y = hardMixMode(v1.y, v2.y);
	ret.z = hardMixMode(v1.z, v2.z);
	ret.a = hardMixMode(v1.a, v2.a);
	return ret;
}

/**************************Difference Mode****************************/
//差值
float4 BlendDifference(float4 v1, float4 v2)
{
	return abs(v1 - v2);
}

//排除
float4 BlendExclusion(float4 v1, float4 v2)
{
	return v1 + v2 - v1 * v2 * 0.5;
}

//减去
float4 BlendSubtract(float4 v1, float4 v2)
{
	return (v1 - v2) * 2;
}

//相加
float4 BlendAdd(float4 v1, float4 v2)
{
	return (v1 + v2) * 0.5;
}

//溶解
float4 BlendDissolve(float4 v1, float4 v2, float2 pos)
{
	float alpha = saturate(snoise(pos));
	return v1 * alpha + (1 - alpha) * v2;
}

#ifdef UPAINTER_LAYER_BLEND_NORMAL
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendNormal(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_RESTORE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendRestore(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_REPLACE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendReplace(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_DARKEN
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendDarken(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_MUTIPY
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendMutipy(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_COLORBURN
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendColorBurn(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_LINEARDARK
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendLinearDark(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_LIGHTEN
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendLighten(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_COLORSCREEN
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendColorScreen(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_COLORDODGE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendColorDodge(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_LINEARDODGE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendLinearDodge(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_OVERLAY
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendOverlay(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_HARDLIGHT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendHardLight(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_SOFTLIGHT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendSoftLight(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_VIVIDLIGHT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendVividLight(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_PINLIGHT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendPinLight(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_LINEARLIGHT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendLinearLight(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_HARDMIX
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendHardMix(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_DIFFERENCE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendDifference(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_EXCLUSION
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendExclusion(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_SUBTRACT
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendSubtract(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_ADD
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendAdd(baseColor, overlayColor)
#elif UPAINTER_LAYER_BLEND_DISSOLVE
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendDissolve(baseColor, overlayColor, pos)
#else
	#define UPAINTER_LAYER_BLEND(baseColor, overlayColor, pos) BlendNormal(baseColor, overlayColor)
#endif

#endif //UPAINTER_BLEND