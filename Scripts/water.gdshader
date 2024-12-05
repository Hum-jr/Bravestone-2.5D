shader_type spatial;

const float SMOOTHSTEP_AA = 0.01;
uniform sampler2D surfaceNoise;
uniform sampler2D distortNoise;
uniform sampler2D foamNoise;

uniform float foam_distance = 0.01;
uniform float foam_max_distance = 0.4;
uniform float foam_min_distance = 0.04;
uniform vec4 foam_color = vec4(1.0);

uniform vec2 surface_noise_tiling = vec2(1.0, 4.0);
uniform vec3 surface_noise_scroll = vec3(0.03, 0.03, 0.0);
uniform float surface_noise_cutoff = 0.777;
uniform float surface_distortion_amount = 0.27;

uniform vec4 water_color = vec4(0.0, 0.5, 1.0, 1.0);
uniform vec4 white_line_color = vec4(1.0, 1.0, 1.0, 1.0);

varying vec2 noiseUV;
varying vec2 distortUV;
varying vec3 viewNormal;

vec4 alphaBlend(vec4 top, vec4 bottom)
{
    vec3 color = (top.rgb * top.a) + (bottom.rgb * (1.0 - top.a));
    float alpha = top.a + bottom.a * (1.0 - top.a);
    return vec4(color, alpha);
}

void vertex() {
    viewNormal = (MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
    noiseUV = UV * surface_noise_tiling;
    distortUV = UV;
}

void fragment() {
    vec4 distortNoiseSample = texture(distortNoise, distortUV);
    vec2 distortAmount = (distortNoiseSample.xy * 2.0 - 1.0) * surface_distortion_amount;
    
    vec2 curvedUV = noiseUV;
    curvedUV.x += sin(curvedUV.y * 10.0 + TIME * 2.0) * 0.2;  // Increase the amplitude of the sine function

    vec2 noise_uv = vec2(
        (curvedUV.x + TIME * surface_noise_scroll.x) + distortAmount.x, 
        (curvedUV.y + TIME * surface_noise_scroll.y + distortAmount.y)
    );
    float surfaceNoiseSample = texture(surfaceNoise, noise_uv).r;
    float surfaceNoiseAmount = smoothstep(surface_noise_cutoff - SMOOTHSTEP_AA, surface_noise_cutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

    // Foam calculation
    float foamDepth = clamp(1.0 - length(VERTEX.y) / foam_distance, 0.0, 1.0);
    float foamFactor = smoothstep(foam_min_distance, foam_max_distance, foamDepth);
    vec4 foamColor = foam_color * foamFactor;
    foamColor.a *= surfaceNoiseAmount;

    // White lines (streaks) on the Y-axis
    float whiteLines = abs(sin((VERTEX.y + TIME) * 10.0)) * 0.5;
    vec4 whiteLineColor = white_line_color * whiteLines;
    whiteLineColor.a *= surfaceNoiseAmount;

    vec4 color = alphaBlend(foamColor, water_color);
    color = alphaBlend(whiteLineColor, color);

    ALBEDO = color.rgb;
    ALPHA = color.a;
}
