#ifndef _INCLUDE_ADDNOISE_HLSL_
#define _INCLUDE_ADDNOISE_HLSL_

#include "AddNoise.compute"

void addNoise_float(float3 pos, float4x4 paramMatrix, out float noise)
{
    NoiseParameters params;
    params.octaves = paramMatrix[0][0];
    params.startingFrequency = paramMatrix[1][0];
    params.frequencyStep = paramMatrix[1][1];
    params.startingAmplitude = paramMatrix[1][2];
    params.amplitudeStep = paramMatrix[1][3];
    params.noiseOffset = paramMatrix[2];
    params.noiseScalar = paramMatrix[3];

    noise = addNoise(pos, params);
}

#endif