# Unity Shader Performance Metrics

Offline compilation and analysis of ShaderLab and ShaderGraph shaders, using the Mali Offline Shader Compiler.

<p align="center">
  <img width="100%" src="https://github.com/eldnach/shader-perf-metrics/blob/main/.github/images/shader-perf-metrics.png?raw=true" alt="ShaderPerf">
</p>

Additional shader compilers may be supported in the future.

## Requirements
Supported in Unity 2021 LTS and later. Pre-installation of the Mali Offline Shader Compiler is required: https://developer.arm.com/Tools%20and%20Software/Mali%20Offline%20Compiler

## Setup
1. In the Unity Editor, go to `Window > Package Manager`
2. On the top left on the Package Manager window, click on `+ > Add package from git URL...` 
3. Add the following URL "https://github.com/eldnach/unity-shader-perf.git" and click `Add`

Once installed, go to `Window > Shader Performance Metrics > Open`.

## Shader Settings
`Shader`: Select a valid ShaderLab or ShaderGraph shader to compile and anaylize.  
`Subshader`: Select the relevant Subshader index, out of the selected shader's available subshaders.  
`Pass`: Select the relevant Pass index, out of the selected subshader's available passes.  
`Keywords`: Add a list of shader keywords to enable, in order to compile and analyze a specific shader variant. Refer to the Unity documentation for more information on [shader variants](https://docs.unity3d.com/Manual/shader-variants.html).

## Compiler Settings
`Compiler`: Select the desired offline shader compiler to use. At the moment, the only supported compiler is "MALIOC".  
`Compiler Path`: Specify the absolute path to the selected compiler's executable.  
`GPU`: Select a target GPU provided by the selected compiler.  
`Graphics API`: Select a target graphics API.  
`Build Target`: Select a Unity build target.  

## Compiler Report
`Source/Disassembly`: View the shader's source (glsl) or disassembly (spir-v).   
`Metrics`: Instruction cycle counts reported by the compiler (arithmetic, load/store, texture...).      
`Resources and Properties`: Additional insights reported by the compiler (register usage, 16bit-arithmetic, late z-test...).   
