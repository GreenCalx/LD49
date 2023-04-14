#ifndef SCHNIBBLE_CUSTOM_OIT_INCLUDED
#define SCHNIBBLE_CUSTOM_OIT_INCLUDED

struct OITFragDepth {
    float4 color;
    float depth;
    uint  next;
};

#define MAX_DEPTH_PER_FRAG 8

// linked list of fragment depth
RWStructuredBuffer<OITFragDepth> _oitFragDepth : register(u1);
// list of first index for each fragment
RWStructuredBuffer<uint> _oitFragHeadIdx : register(u2);

inline uint GetFragIdx(float2 screenPos) {
    float2 screenPosPx = screenPos * _ScreenParams.xy;
    return (uint)screenPosPx.x + (uint)screenPosPx.y * _ScreenParams.x;
}

void AddOITValue(float2 screenPos, float4 color, float depth){
    uint fragIdx = GetFragIdx(screenPos);

    uint nextIdx = _oitFragDepth.IncrementCounter();
    uint previousIdx;
    InterlockedExchange(_oitFragHeadIdx[fragIdx], nextIdx, previousIdx);

    OITFragDepth newValue;
    newValue.color = color;
    newValue.depth = depth;
    newValue.next  = previousIdx;

    _oitFragDepth[nextIdx] = newValue;
}

void ResolveOIT(float2 screenPos, inout float4 color) {
    uint fragIdx = GetFragIdx(screenPos);
    uint headIdx = _oitFragHeadIdx[fragIdx];

    OITFragDepth sortedDepth[MAX_DEPTH_PER_FRAG];

    int fragDepthCount=0;
    while (headIdx != 0)
    {
        OITFragDepth current = _oitFragDepth[headIdx];
        sortedDepth[fragDepthCount] = current;
        fragDepthCount += 1;

        headIdx = (fragDepthCount >= MAX_DEPTH_PER_FRAG) ? 0 : current.next;
    }

    // Sort pixels in depth
    // todo toffa : better sort alg than O(n2)
    // i suspect that having always a sorted list per frag might be better than a linked list
    for (int i = 0; i < fragDepthCount; ++i)
    {
        for (int j = i + 1; j < fragDepthCount; ++j)
        {
            float refDepth = sortedDepth[i].depth;
            float curDepth = sortedDepth[j].depth;
            if (curDepth > refDepth)
            {
                // swap
                OITFragDepth t = sortedDepth[j];
                sortedDepth[j] = sortedDepth[i];
                sortedDepth[i] = t;
            }
        }
    }

    // Rendering pixels
    for (int k = 0; k < fragDepthCount; k++)
    {
        float4 currColor = sortedDepth[k].color;
        color.rgb = lerp(color.rgb, currColor.rgb, currColor.a);
    }
}
#endif
