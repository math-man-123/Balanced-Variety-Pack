shader_type canvas_item;

uniform sampler2D screen_texture
    : hint_screen_texture, repeat_disable, filter_linear;
uniform float chroma_size;
uniform float chroma_speed;


float hash(float t) {
    // cheap pseudo-random hash
    return fract(sin(t) * 43758.5453123);
}

float smooth_noise(float t) {
    float i = floor(t);
    float f = fract(t);
    
    // linear interpolation mix(a,b,t) = a(1−t)+bt
    // edges of [i, i+1] smoothed via 3f^3 - 2f^2 
    f = f * f * (3.0 - 2.0 * f);
    return mix(hash(i), hash(i + 1.0), f);
}

vec2 random_offset(float t, float seed) {
    // generate random x and y offset 
    // then map [0, 1] into [-1, 1]
    return vec2(
        smooth_noise(t + seed),
        smooth_noise(t + seed + 123.456)
    ) * 2.0 - 1.0;
}

void fragment()
{
    vec4 color = texture(screen_texture, SCREEN_UV);
    float t = TIME * chroma_speed;

    vec2 offset_r = random_offset(t, 10.0);
    vec2 offset_g = random_offset(t, 20.0);
    vec2 offset_b = random_offset(t, 30.0);

    vec2 scale = SCREEN_PIXEL_SIZE * chroma_size;

    color.r = texture(screen_texture, SCREEN_UV + scale * offset_r).r;
    color.g = texture(screen_texture, SCREEN_UV + scale * offset_g).g;
    color.b = texture(screen_texture, SCREEN_UV + scale * offset_b).b;

    COLOR = color;
}
