shader_type spatial;

#include "res://addons/F00F.ShaderNoise/FastNoiseLite.gdshaderinc"

//#define USE_NOISE
//#define USE_HEIGHTMAP
//#define USE_NORMALMAP
//#define USE_OVERLAY
//#define USE_BLENDING
//#define USE_GRADIENT

#ifndef REGION_COUNT
#define REGION_COUNT 8
#endif

uniform vec3 tints[REGION_COUNT];
uniform sampler2D textures[REGION_COUNT]; // (editor warning)

uniform float min_slopes[REGION_COUNT];
uniform float max_slopes[REGION_COUNT];
uniform float min_heights[REGION_COUNT];
uniform float max_heights[REGION_COUNT];
uniform float texture_scales[REGION_COUNT];
uniform float tint_strengths[REGION_COUNT];
uniform float blend_strengths[REGION_COUNT];

uniform int region_count = REGION_COUNT;

uniform sampler2D height_curve;
uniform sampler2D height_map;
uniform sampler2D normal_map;
uniform sampler2D overlay;
uniform float amplitude;

uniform float lod_step;
uniform float chunk_size;
uniform float chunk_scale;

uniform vec2 terrain_pos;


/////////////////////
// -- UTILITIES -- //
/////////////////////

const float epsilon = 0.000001;

float true_round(float value) {
    return floor(value + 0.5);
}

float inverse_lerp(float value, float min, float max) {
    return (value - min) / (max - min);
}

float binary_step(float value, float min, float max) {
    return float(value >= min && value <= max);
}

float weight(float value, float min, float max) {
    float w = max(inverse_lerp(value, min, max), 0);
    return w * max(sign(max - value + epsilon), 0);
}

float bell_weight(float value, float min, float max) {
    float w = weight(value, min, max);
    return 4.0 * w * (1.0 - w);
}

vec4 texture_value(sampler2D tex, vec2 pos) {
    vec2 tex_size = vec2(textureSize(tex, 0));
    vec2 tex_offset = tex_size * 0.5;
    vec2 tex_position = (pos + 0.5 + tex_offset) / tex_size;
    return texture(tex, tex_position);
}

vec4 triplane_texture(sampler2D source, vec3 scaled_position, vec3 weighted_normal) {
    return
        texture(source, scaled_position.yz) * weighted_normal.x +
        texture(source, scaled_position.xz) * weighted_normal.y +
        texture(source, scaled_position.xy) * weighted_normal.z;
}

vec3 WorldVertex(vec3 vertex, mat4 model_matrix) {
    return vertex + model_matrix[3].xyz;
}

vec3 WorldFragment(vec3 fragment, mat4 inv_view_matrix) {
    return (inv_view_matrix * vec4(fragment, 0)).xyz;
}


/////////////////
// -- NOISE -- //
/////////////////

#ifdef USE_NOISE

float GetNoiseHeight(vec2 pos) {
    float noise = get_noise(pos.x, pos.y);
    return (noise + 1.0) / 2.0;
}

#else

float GetNoiseHeight(vec2 pos) {
    return 1.0;
}

#endif


/////////////////////
// -- HEIGHTMAP -- //
/////////////////////

#ifdef USE_HEIGHTMAP

float GetImageHeight(vec2 pos) {
    return texture_value(height_map, pos).r;
}

#else

float GetImageHeight(vec2 pos) {
    return 1.0;
}

#endif


///////////////////
// -- OVERLAY -- //
///////////////////

#ifdef USE_OVERLAY

float GetOverlayHeight(vec2 pos) {
    return texture_value(overlay, pos).r;
}

#else

float GetOverlayHeight(vec2 pos) {
    return 1.0;
}

#endif


////////////////////
// -- BLENDING -- //
////////////////////

#ifdef USE_BLENDING

float Blend(float value, float _min, float _max, float blend) {
    blend = (_max - _min) * blend * 0.5;
    return bell_weight(value, _min - blend, _max + blend);
}

#else

float Blend(float value, float _min, float _max, float unused) {
    return binary_step(value, _min, _max);
}

#endif


////////////////////
// -- GRADIENT -- //
////////////////////

#ifdef USE_GRADIENT

float GetGradient(float height) {
    return texture(height_curve, vec2(height, 0)).r;
}

#else

float GetGradient(float height) {
    return 1.0;
}

#endif


/////////////////////
// -- FUNCTIONS -- //
/////////////////////

float GetLod(vec2 pos) {
    vec2 local_pos = pos - terrain_pos;
    float lod = true_round(max(abs(local_pos.x), abs(local_pos.y)) / chunk_size) * lod_step;
    return min(pow(2, lod), chunk_size);
}

float GetRawHeight(vec2 pos) {
    return GetNoiseHeight(pos) * GetImageHeight(pos) * GetOverlayHeight(pos);
}

float GetHeight(vec2 pos) {
    float height = GetRawHeight(pos);
    float gradient = GetGradient(height);
    return height * gradient * amplitude * chunk_scale;
}

float GetStitchedHeight(vec2 pos) {
    float lod = GetLod(pos);
    vec2 fraction = fract((pos + chunk_size * 0.5) / lod);

    float x_height = mix(
        GetHeight(pos - vec2(fraction.x * lod, 0)),
        GetHeight(pos + vec2((1.0 - fraction.x) * lod, 0)),
        fraction.x);

    float y_height = mix(
        GetHeight(pos - vec2(0, fraction.y * lod)),
        GetHeight(pos + vec2(0, (1.0 - fraction.y) * lod)),
        fraction.y);

    return mix(x_height, y_height, ceil(fraction.y));
}

vec3 BlendTextures(vec3 vertex, vec3 normal) {
    vec3 weighted_normal = abs(normal);
    weighted_normal /= weighted_normal.x + weighted_normal.y + weighted_normal.z;

    vec3 result = vec3(0);
    float slope = 1.0 - abs(normal.y);
    float height = GetRawHeight(vertex.xz);

    for (int i = 0; i < region_count; ++i) {
        float min_slope = min_slopes[i];
        float max_slope = max_slopes[i];
        float min_height = min_heights[i];
        float max_height = max_heights[i];
        float texture_scale = texture_scales[i];
        float tint_strength = tint_strengths[i];
        float blend_strength = blend_strengths[i];
        vec3 scaled_position = vertex / texture_scale;

        vec3 tint = tints[i] * tint_strength;
        vec3 tex = triplane_texture(textures[i], scaled_position, weighted_normal).rgb * (1.0 - tint_strength);

        float slope_weight = Blend(slope, min_slope, max_slope, blend_strength);
        float height_weight = Blend(height, min_height, max_height, blend_strength);

        float weight = slope_weight * height_weight;
        result = result * (1.0 - weight) + (tint + tex) * weight;
    }

    return result;
}


///////////////////
// -- NORMALS -- //
///////////////////

#ifdef USE_NORMALMAP

vec3 GetNormalMap(vec2 pos) {
    return texture_value(normal_map, pos).rgb;
}

vec3 FromNormalMap(vec3 m) {
    m = vec3(m.x, m.z, m.y);
    m = m * 2.0 - 1.0;
    m *= vec3(1, 1, -1);
    return m;
}

vec3 GetNormalMap(vec3 vertex, out vec3 normal) {
    vec3 map = GetNormalMap(vertex.xz);
    normal = FromNormalMap(map);
    return map;
}

#else

vec3 GetNormal(vec3 vertex) {
    vec3 east_vertex = vertex + vec3(1, 0, 0);
    vec3 west_vertex = vertex + vec3(-1, 0, 0);
    vec3 north_vertex = vertex + vec3(0, 0, 1);
    vec3 south_vertex = vertex + vec3(0, 0, -1);

    east_vertex.y = GetHeight(east_vertex.xz);
    west_vertex.y = GetHeight(west_vertex.xz);
    north_vertex.y = GetHeight(north_vertex.xz);
    south_vertex.y = GetHeight(south_vertex.xz);

    vec3 dx = east_vertex - west_vertex;
    vec3 dy = north_vertex - south_vertex;
    return normalize(cross(dy, dx));
}

vec3 ToNormalMap(vec3 n) {
    n *= vec3(1, 1, -1);
    n = n * 0.5 + 0.5;
    n = vec3(n.x, n.z, n.y);
    return n;
}

vec3 GetNormalMap(vec3 vertex, out vec3 normal) {
    normal = GetNormal(vertex);
    return ToNormalMap(normal);
}

#endif


////////////////
// -- MAIN -- //
////////////////

varying vec3 world_vertex;

void vertex() {
    world_vertex = WorldVertex(VERTEX, MODEL_MATRIX);
    world_vertex.y = GetStitchedHeight(world_vertex.xz);
    VERTEX.y = world_vertex.y;
}

void fragment() {
    vec3 normal;
    NORMAL_MAP = GetNormalMap(world_vertex, normal);
    ALBEDO = BlendTextures(world_vertex, normal);
}
