uniform float4 gl_HalfPixel;

static float4 gl_Position;
static float2 vin_position;
static float2 fin_texcoord;
static float2 vin_texcoord;
static float4 fin_color;
static float4 vin_color;

struct SPIRV_Cross_Input
{
    float2 vin_position : TEXCOORD0;
    float2 vin_texcoord : TEXCOORD1;
    float4 vin_color : TEXCOORD2;
};

struct SPIRV_Cross_Output
{
    float2 fin_texcoord : TEXCOORD0;
    float4 fin_color : TEXCOORD1;
    float4 gl_Position : POSITION;
};

void vert_main()
{
    gl_Position = float4(float3(vin_position, 1.0f).xy, 0.0f, 1.0f);
    fin_texcoord = vin_texcoord;
    fin_color = vin_color;
    gl_Position.y = -gl_Position.y;
    gl_Position.x = gl_Position.x - gl_HalfPixel.x * gl_Position.w;
    gl_Position.y = gl_Position.y + gl_HalfPixel.y * gl_Position.w;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    vin_position = stage_input.vin_position;
    vin_texcoord = stage_input.vin_texcoord;
    vin_color = stage_input.vin_color;
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output.fin_texcoord = fin_texcoord;
    stage_output.fin_color = fin_color;
    return stage_output;
}
