#if defined(UNIVERSAL_LIGHTING_INCLUDED)
    Light light = GetMainLight();
    Direction = light.direction;
#else
    Direction = float3(0.5, 0.5, 0.5);
#endif
