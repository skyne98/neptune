#version 310 es
precision mediump float;

in vec2 vin_position;
in vec2 vin_texcoord;
in vec4 vin_color;

out vec2 fin_texcoord;
out vec4 fin_color;

void main()
{
    gl_Position = vec4((vec3(vin_position, 1.0)).xy, 0.0, 1.0);
	fin_texcoord = vin_texcoord;
    fin_color = vin_color;
}