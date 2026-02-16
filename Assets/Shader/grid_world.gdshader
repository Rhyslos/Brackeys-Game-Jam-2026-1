shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 background_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);
uniform vec4 main_grid_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform float grid_scale = 1.0; // 1.0 = 1 meter usually
uniform float main_line_thickness : hint_range(0.001, 0.1) = 0.02;

uniform int subdivisions : hint_range(1, 10) = 4; // 2 = 2x2, 4 = 4x4, etc.
uniform vec4 sub_grid_color : source_color = vec4(0.3, 0.3, 0.3, 1.0);
uniform float sub_line_thickness : hint_range(0.001, 0.1) = 0.01;

uniform float line_sharpness : hint_range(0.0, 1.0) = 0.0;

varying vec3 world_pos;
varying vec3 world_normal;

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
}

float get_grid_intensity(vec3 pos, float scale, float thickness, float softness, vec3 normal_weights) {
    vec3 pos_scaled = pos * scale;
    
    vec2 grid_x = abs(fract(pos_scaled.yz - 0.5) - 0.5);
    vec2 grid_y = abs(fract(pos_scaled.xz - 0.5) - 0.5);
    vec2 grid_z = abs(fract(pos_scaled.xy - 0.5) - 0.5);

    float lines_x = smoothstep(thickness, thickness - softness, min(grid_x.x, grid_x.y));
    float lines_y = smoothstep(thickness, thickness - softness, min(grid_y.x, grid_y.y));
    float lines_z = smoothstep(thickness, thickness - softness, min(grid_z.x, grid_z.y));

    return lines_x * normal_weights.x + lines_y * normal_weights.y + lines_z * normal_weights.z;
}

void fragment() {
    vec3 weights = abs(world_normal);
    weights /= (weights.x + weights.y + weights.z);
    
    float edge_softness = 0.002 + (1.0 - line_sharpness) * 0.02;
    float main_grid_mix = get_grid_intensity(world_pos, grid_scale, main_line_thickness, edge_softness, weights);
	float sub_scale = grid_scale * float(subdivisions);
    float sub_grid_mix = get_grid_intensity(world_pos, sub_scale, sub_line_thickness, edge_softness, weights);

    vec3 out_color = background_color.rgb;
    
    out_color = mix(out_color, sub_grid_color.rgb, sub_grid_mix);
    out_color = mix(out_color, main_grid_color.rgb, main_grid_mix);

    ALBEDO = out_color;
    ROUGHNESS = 0.8;
    METALLIC = 0.0;
}