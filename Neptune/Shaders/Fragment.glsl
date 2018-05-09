#version 310 es
precision mediump float;

layout(location = 0) in vec2 fin_texcoord;
layout(location = 1) in vec4 fin_color;

layout(binding=0) uniform sampler2D sprite_texture;

layout(location = 0) out vec4 fout_color;

void main()
{
    fout_color = fin_color * texture(sprite_texture, fin_texcoord);
}
