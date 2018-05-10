#version 310 es
precision mediump float;

in vec2 fin_texcoord;
in vec4 fin_color;

layout(binding=0) uniform sampler2D sprite_texture;

out vec4 fout_color;

void main()
{
    fout_color = fin_color * texture(sprite_texture, fin_texcoord);
}
