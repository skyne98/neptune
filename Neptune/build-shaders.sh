#!/usr/bin/env bash

# Compile to SPIR-V
./Utilities/glslang/glslangValidator -V -S vert ./Shaders/Vertex.glsl -o ./Shaders/Vertex.spv
./Utilities/glslang/glslangValidator -V -S frag ./Shaders/Fragment.glsl -o ./Shaders/Fragment.spv

# Make optimizations
./Utilities/opt/spirv-opt ./Shaders/Vertex.spv -o ./Shaders/Vertex.spv
./Utilities/opt/spirv-opt ./Shaders/Fragment.spv -o ./Shaders/Fragment.spv

# Compile to GLSL.450
./Utilities/cross/spirv-cross ./Shaders/Vertex.spv --flip-vert-y --stage vert --output ./Shaders/Vertex.450.glsl
./Utilities/cross/spirv-cross ./Shaders/Fragment.spv --flip-vert-y --stage frag --output ./Shaders/Fragment.450.glsl

# Compile back to SPIR-V
./Utilities/glslang/glslangValidator -V -S vert ./Shaders/Vertex.450.glsl -o ./Shaders/Vertex.spv
./Utilities/glslang/glslangValidator -V -S frag ./Shaders/Fragment.450.glsl -o ./Shaders/Fragment.spv

# Make optimizations
./Utilities/opt/spirv-opt ./Shaders/Vertex.spv -o ./Shaders/Vertex.spv
./Utilities/opt/spirv-opt ./Shaders/Fragment.spv -o ./Shaders/Fragment.spv

# Compile to HLSL
./Utilities/cross/spirv-cross ./Shaders/Vertex.spv --hlsl --stage vert --output ./Shaders/Vertex.hlsl
./Utilities/cross/spirv-cross ./Shaders/Fragment.spv --hlsl --stage frag --output ./Shaders/Fragment.hlsl