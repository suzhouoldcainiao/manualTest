#pragma once
#ifndef _OFILMEOLTEST_H_
#define _OFILMEOLTEST_H_

#define OFILMEOLTEST_API extern "C" __declspec(dllexport)

#define  ABS_WHITE 255
#define  ABS_BLACK 0

#define  SFR_CHART_NUM	5						//SFR �����趨��Chart ����
#define  COLOR_ROI_NUM	6						//DeltaE/C �����趨��ROI ����


/************************DefineErr****************************/
#define SFR_CNT_OVER_SETTING						-100
#define COLOR_CNT_OVER_SETTING						-110


/************************************************************/

struct SINGLECHART
{
	//INPUT
	RECT SearchRT;								//��λ��
	double m_dFreq;								//�ռ�Ƶ��
	int m_nBinaryThrehoid;						//��ֵ��
	int m_nRoiDistance;							//Roi��������ĵľ���
	int m_nPatternSize;							//����ʶ���С
	int m_nRoiW;								//Roi��
	int m_nRoiH;								//Roi��
	int m_nRoiEnable[4];						//�ĸ�Roi�Ŀ���
	
	bool m_bUseMTF;								//0:����sfr,1:����MTF
	int m_nMtfPer;								//����MTFʱ�� MTF�Ļ�׼

	//OUTPUT
	double m_dOCx;									//����chart�����ĵ�X����
	double m_dOCy;									//����chart�����ĵ�Y����
	double m_dSfrValue[4];						//����chart������Roi������ķ��� 0-3 ��Ӧ�¡��ҡ��ϡ���
	double m_dSfrAvgValue;						//����chart�ļ�������ܷ���ƽ��ֵ
	int m_nLightValue;							//����chart�İ�ɫ���������ֵ

	SINGLECHART() :m_dFreq(0), m_nBinaryThrehoid(0), m_nRoiDistance(0), m_nPatternSize(0), m_nRoiW(0), m_nRoiH(0), m_nRoiEnable{ 0 }, m_bUseMTF(0), m_nMtfPer(0), m_dOCx(0), m_dOCy(0), m_dSfrValue{0}, m_dSfrAvgValue(0), m_nLightValue(0)
	{

	}
};

struct COLOR_TEST
{
	//INPUT
	RECT rectRoi;								//Roi������
	int nStandR;								//��׼ɫR����
	int nStandG;								//��׼ɫG����
	int nStandB;								//��׼ɫB����

	//OUTPUT
	float fValue;								//����ֵ


	COLOR_TEST() :nStandR(0), nStandG(0), nStandB(0),fValue(0)
	{

	}
};

//�������ʱ����һ�μ���
OFILMEOLTEST_API void InitLib();				

OFILMEOLTEST_API int SfrTest(BYTE* bmp, int nimgW, int nimgH, SINGLECHART* sSigleChart);

OFILMEOLTEST_API int ColorTest(BYTE* bmp, int nimgW, int nimgH, COLOR_TEST* sColor, int nMode);










#endif