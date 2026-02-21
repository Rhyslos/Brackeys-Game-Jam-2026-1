shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

// uniform variables
uniform vec4 albedo : source_color = vec4(0.2, 0.8, 0.2, 0.85);
uniform vec4 edge_color : source_color = vec4(0.6, 1.0, 0.4, 1.0);
uniform float edge_scale = 3;

uniform sampler2D noise_tex_1;
uniform sampler2D noise_tex_2;
uniform vec2 scroll_speed_1 = vec2(0.02, 0.03);
uniform vec2 scroll_speed_2 = vec2(-0.03, 0.02);
uniform float uv_scale = 0.1;

uniform vec4 emission_color : source_color = vec4(0.2, 0.9, 0.2, 1.0);
uniform float emission_energy = 3.5
;
uniform float pulse_speed = 2.0;
uniform float pulse_strength = 0.4;
uniform float roughness_val = 0.1;

uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;

// varying variables
varying vec3 world_pos;

// vertex functions
void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

// fragment functions
void fragment() {
    vec2 noise_uv_1 = world_pos.xz * uv_scale + TIME * scroll_speed_1;
    vec2 noise_uv_2 = world_pos.xz * uv_scale + TIME * scroll_speed_2;

    float n1 = texture(noise_tex_1, noise_uv_1).r;
    float n2 = texture(noise_tex_2, noise_uv_2).r;
    float swirl = n1 * n2 * 2.0;

    float depth_val = texture(depth_texture, SCREEN_UV).x;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth_val);
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float linear_depth = -view.z;
    float fade = clamp((linear_depth + VERTEX.z) / edge_scale, 0.0, 1.0);

    vec3 current_albedo = mix(edge_color.rgb, albedo.rgb, fade);
    current_albedo = mix(current_albedo, current_albedo * 1.5, swirl);

    float pulse = (sin(TIME * pulse_speed) * 0.5 + 0.5) * pulse_strength;
    vec3 current_emission = mix(edge_color.rgb, emission_color.rgb, fade);
    current_emission = current_emission * emission_energy * swirl * (1.0 + pulse);

    ALBEDO = current_albedo;
    EMISSION = current_emission;
    ROUGHNESS = roughness_val;
    ALPHA = albedo.a;
}