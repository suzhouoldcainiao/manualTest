#pragma once
#ifndef _OFILMEOLTEST_H_
#define _OFILMEOLTEST_H_

#define OFILMEOLTEST_API extern "C" __declspec(dllexport)

#define  ABS_WHITE 255
#define  ABS_BLACK 0

#define  SFR_CHART_NUM	5						//SFR 测试设定的Chart 数量
#define  COLOR_ROI_NUM	6						//DeltaE/C 测试设定的ROI 数量


/************************DefineErr****************************/
#define SFR_CNT_OVER_SETTING						-100
#define COLOR_CNT_OVER_SETTING						-110


/************************************************************/

struct SINGLECHART
{
	//INPUT
	RECT SearchRT;								//定位框
	double m_dFreq;								//空间频率
	int m_nBinaryThrehoid;						//二值化
	int m_nRoiDistance;							//Roi相对于中心的距离
	int m_nPatternSize;							//框体识别大小
	int m_nRoiW;								//Roi宽
	int m_nRoiH;								//Roi高
	int m_nRoiEnable[4];						//四个Roi的开关
	
	bool m_bUseMTF;								//0:计算sfr,1:计算MTF
	int m_nMtfPer;								//计算MTF时的 MTF的基准

	//OUTPUT
	double m_dOCx;									//宝马chart的中心点X坐标
	double m_dOCy;									//宝马chart的中心点Y坐标
	double m_dSfrValue[4];						//宝马chart的四周Roi计算出的分数 0-3 对应下、右。上、左
	double m_dSfrAvgValue;						//宝马chart的计算出的总分数平均值
	int m_nLightValue;							//宝马chart的白色区域的亮度值

	SINGLECHART() :m_dFreq(0), m_nBinaryThrehoid(0), m_nRoiDistance(0), m_nPatternSize(0), m_nRoiW(0), m_nRoiH(0), m_nRoiEnable{ 0 }, m_bUseMTF(0), m_nMtfPer(0), m_dOCx(0), m_dOCy(0), m_dSfrValue{0}, m_dSfrAvgValue(0), m_nLightValue(0)
	{

	}
};

struct COLOR_TEST
{
	//INPUT
	RECT rectRoi;								//Roi搜索框
	int nStandR;								//标准色R分量
	int nStandG;								//标准色G分量
	int nStandB;								//标准色B分量

	//OUTPUT
	float fValue;								//测试值


	COLOR_TEST() :nStandR(0), nStandG(0), nStandB(0),fValue(0)
	{

	}
};

//软件启动时调用一次即可
OFILMEOLTEST_API void InitLib();				

OFILMEOLTEST_API int SfrTest(BYTE* bmp, int nimgW, int nimgH, SINGLECHART* sSigleChart);

OFILMEOLTEST_API int ColorTest(BYTE* bmp, int nimgW, int nimgH, COLOR_TEST* sColor, int nMode);










#endif