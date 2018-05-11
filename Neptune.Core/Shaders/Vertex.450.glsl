#version 310 es

layout(location = 0) in mediump vec2 vin_position;
layout(location = 0) out mediump vec2 fin_texcoord;
layout(location = 1) in mediump vec2 vin_texcoord;
layout(location = 1) out mediump vec4 fin_color;
layout(location = 2) in mediump vec4 vin_color;

void main()
{
    gl_Position = vec4(vec3(vin_position, 1.0).xy, 0.0, 1.0);
    fin_texcoord = vin_texcoord;
    fin_color = vin_color;
    gl_Position.y = -gl_Position.y;
}

