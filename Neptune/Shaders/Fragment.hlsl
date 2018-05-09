uniform sampler2D sprite_texture;

static float4 fout_color;
static float4 fin_color;
static float2 fin_texcoord;

struct SPIRV_Cross_Input
{
    float2 fin_texcoord : TEXCOORD0;
    float4 fin_color : TEXCOORD1;
};

struct SPIRV_Cross_Output
{
    float4 fout_color : COLOR0;
};

void frag_main()
{
    fout_color = fin_color * tex2D(sprite_texture, fin_texcoord);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    fin_color = stage_input.fin_color;
    fin_texcoord = stage_input.fin_texcoord;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.fout_color = fout_color;
    return stage_output;
}
