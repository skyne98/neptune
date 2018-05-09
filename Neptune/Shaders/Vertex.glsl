#version 310 es
precision mediump float;

layout(location = 0) in vec2 vin_position;
layout(location = 1) in vec2 vin_texcoord;
layout(location = 2) in vec4 vin_color;

layout(location = 0) out vec2 fin_texcoord;
layout(location = 1) out vec4 fin_color;

void main()
{
    gl_Position = vec4((vec3(vin_position, 1.0)).xy, 0.0, 1.0);
	fin_texcoord = vin_texcoord;
    fin_color = vin_color;
}