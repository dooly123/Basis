// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.CompositionLayer;

public class ColorScaleBiasManager : MonoBehaviour
{
    public Slider colorScaleSliderR, colorScaleSliderG, colorScaleSliderB, colorScaleSliderA;
    public Image colorScalePreviewRGB, colorScalePreviewA;

    public Slider colorBiasSliderR, colorBiasSliderG, colorBiasSliderB, colorBiasSliderA;
    public Image colorBiasPreviewRGB, colorBiasPreviewA;

    public RawImage colorScaleBiasAppliedPreview;

    public List<CompositionLayer> compositionLayerList;

    private Color colorScaleResult, colorScaleResultRGB, colorScaleResultA, colorBiasResult, colorBiasResultRGB, colorBiasResultA;
    private readonly Color defaultColorScale = Color.white, defaultColorBias = Color.clear;


    private void Awake()
    {
        foreach (CompositionLayer compositionLayer in compositionLayerList)
        {
            colorScaleSliderR.value = compositionLayer.colorScale.r;
            colorScaleSliderG.value = compositionLayer.colorScale.g;
            colorScaleSliderB.value = compositionLayer.colorScale.b;
            colorScaleSliderA.value = compositionLayer.colorScale.a;

            colorBiasSliderR.value = compositionLayer.colorBias.r;
            colorBiasSliderG.value = compositionLayer.colorBias.g;
            colorBiasSliderB.value = compositionLayer.colorBias.b;
            colorBiasSliderA.value = compositionLayer.colorBias.a;
        }

        colorScaleResultRGB.a = colorBiasResultRGB.a = 1;
        colorScaleResultA.r = colorBiasResultA.r = 1;
        colorScaleResultA.g = colorBiasResultA.g = 1;
        colorScaleResultA.b = colorBiasResultA.b = 1;
    }

    // Update is called once per frame
    void Update()
    {
        colorScaleResult.r = colorScaleSliderR.value;
        colorScaleResult.g = colorScaleSliderG.value;
        colorScaleResult.b = colorScaleSliderB.value;
        colorScaleResult.a = colorScaleSliderA.value;

        colorScaleResultRGB.r = colorScaleResult.r;
        colorScaleResultRGB.g = colorScaleResult.g;
        colorScaleResultRGB.b = colorScaleResult.b;

        colorScaleResultA.a = colorScaleResult.a;

        colorScalePreviewRGB.color = colorScaleResultRGB;
        colorScalePreviewA.color = colorScaleResultA;
        foreach (CompositionLayer compositionLayer in compositionLayerList)
        {
            compositionLayer.colorScale = colorScaleResult;
        }

        colorBiasResult.r = colorBiasSliderR.value;
        colorBiasResult.g = colorBiasSliderG.value;
        colorBiasResult.b = colorBiasSliderB.value;
        colorBiasResult.a = colorBiasSliderA.value;

        colorBiasResultRGB.r = colorBiasResult.r;
        colorBiasResultRGB.g = colorBiasResult.g;
        colorBiasResultRGB.b = colorBiasResult.b;

        colorBiasResultA.a = colorBiasResult.a;

        colorBiasPreviewRGB.color = colorBiasResultRGB;
        colorBiasPreviewA.color = colorBiasResultA;
        foreach (CompositionLayer compositionLayer in compositionLayerList)
        {
            compositionLayer.colorBias = colorBiasResult;
        }

        Color resultColor = Color.white; //original color
        resultColor *= colorScaleResult;
        resultColor += colorBiasResult;

        colorScaleBiasAppliedPreview.color = resultColor;
    }

    public void ResetColorScaleBias()
    {
        colorScaleSliderR.value = defaultColorScale.r;
        colorScaleSliderG.value = defaultColorScale.g;
        colorScaleSliderB.value = defaultColorScale.b;
        colorScaleSliderA.value = defaultColorScale.a;

        colorBiasSliderR.value = defaultColorBias.r;
        colorBiasSliderG.value = defaultColorBias.g;
        colorBiasSliderB.value = defaultColorBias.b;
        colorBiasSliderA.value = defaultColorBias.a;

        foreach (CompositionLayer compositionLayer in compositionLayerList)
        {
            compositionLayer.colorScale = defaultColorScale;
            compositionLayer.colorBias = defaultColorBias;
        }
    }
}
