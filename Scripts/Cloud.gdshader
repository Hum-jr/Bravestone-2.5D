shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_always, depth_test_disabled;

uniform vec4 cloud_color : source_color = vec4(1.0, 1.0, 1.0, 0.8);
uniform float cloud_density : hint_range(0.0, 1.0) = 0.6;
uniform float noise_scale : hint_range(1.0, 10.0) = 3.0;
uniform float noise_detail : hint_range(1.0, 5.0) = 2.0;
uniform float wind_speed : hint_range(0.0, 2.0) = 0.1;
uniform vec2 wind_direction = vec2(0.0, -1.0);

float noise(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float smooth_noise(vec2 p) {
    vec2 lv = fract(p);
    vec2 id = floor(p);

    lv = lv * lv * (3.0 - 2.0 * lv);

    float bl = noise(id);
    float br = noise(id + vec2(1, 0));
    float b = mix(bl, br, lv.x);

    float tl = noise(id + vec2(0, 1));
    float tr = noise(id + vec2(1, 1));
    float t = mix(tl, tr, lv.x);

    return mix(b, t, lv.y);
}

float cloud_noise(vec2 p) {
    float c = 0.0;
    float intensity = 1.0;
    float frequency = noise_scale;

    for (int i = 0; i < int(noise_detail); i++) {
        c += smooth_noise(p * frequency) * intensity;
        frequency *= 2.0;
        intensity *= 0.5;
    }

    return c;
}

void vertex() {
    // Optional vertex manipulation for cloud movement
    vec2 wind_offset = wind_direction * TIME * wind_speed;
    VERTEX.xz += wind_offset;
}

void fragment() {
    // Create cloud texture coordinates with wind movement
    vec2 wind_offset = wind_direction * TIME * wind_speed;
    vec2 uv = UV + wind_offset;

    // Generate cloud noise
    float cloud_noise_value = cloud_noise(uv);

    // Create soft cloud edges and density
    float cloud_coverage = smoothstep(1.0 - cloud_density, 1.0, cloud_noise_value);

    // Final cloud color with transparency
    vec4 final_color = cloud_color;
    final_color.a *= cloud_coverage;

    // Output the final cloud color
    ALBEDO = final_color.rgb;
    ALPHA = final_color.a;
}
