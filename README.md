# Offline Shader Analaysis for Unity

Offline compilation and analysis of ShaderLab and ShaderGraph shaders, using the Mali Offline Shader Compiler.

<p align="center">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/OfflineShaderAnalysis.png?raw=true" alt="OfflineShaderAnalysis">
</p>

Additional shader compilers may be supported in the future.

## Requirements
Supported in Unity 6 LTS and later. Pre-installation of the Mali Offline Shader Compiler is required: https://developer.arm.com/Tools%20and%20Software/Mali%20Offline%20Compiler

## Setup
1. In the Unity Editor, go to `Window > Package Manager`
2. On the top left on the Package Manager window, click on `+ > Add package from git URL...` 
3. Add the following URL "[https://github.com/eldnach/offline-shader-analysis.git](https://github.com/eldnach/offline-shader-analysis.git)" and click `Add`

Once installed, go to `Window > Analysis > Offline Shader Analysis`.

## Example usage

Unity shaders are compiled into thousands of different programs (variants) when building a project. The right program is loaded at runtime based on the active keyword set. This tool allows you to easily pick, compile and analyze individual shader variants.

To start, specify a valid path to the offline shader compiler (MALIOC) and assign a valid Unity Shader for analysis. Configure the Subshader, Pass and Shader Keywords to enable (clicking the little square next to each key). Finally, press "Compile and Report" to analyze the shader.

The package contains a test shader to illustrate the plugin's usage. It incldues 3 custom shader keywords:

`Z_WRITE`: The shader will manually write to the depth buffer in the pixel stage. Doing so will disable hidden surface removal optimizations such as early-z, indicated by the late-z test.
<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/Z_WRITE.png?raw=true" alt="Zwrite">
</p>
<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/ZWriteMetrics.png?raw=true" alt="ZwriteMetrics">
</p>

`STACK_ALLOCATIONS`: The shader will dynamically index into a local array. This will force the compiler to allocate memory on the stack (shared memory) rather than shader registers. The shader is bound by load/store operations.

<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/STACK_ALLOCATIONS.png?raw=true" alt="StackAllocations">
</p>
<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/StackAllocationMetrics.png?raw=true" alt="StackAllocationMetrics">
</p>

`REGISTER_USAGE`: The shader will execute a large number of complex instructions. This will increase shader register usage and reduce occupancy. The shader is bound by arithmetic operations.

<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/REGISTER_USAGE.png?raw=true" alt="RegiterUsage">
</p>
<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/RegisterUsageMetrics.png?raw=true" alt="RegisterUsageMetrics">
</p>

You can also view the generated shader's source or disassembly:
<p align="left">
  <img width="100%" src="https://github.com/eldnach/offline-shader-analysis/blob/main/images/ShaderSource.png?raw=true" alt="ShaderSource">
</p>

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
