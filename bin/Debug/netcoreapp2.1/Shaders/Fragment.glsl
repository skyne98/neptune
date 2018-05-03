#version 330 core

uniform sampler2D tex;

in vec2 fsin_Color;
out vec4 outColor;

void main()
{
    outColor = texture(tex, -fsin_Color.xy);
}
