using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;

public class MaliocPlugin{

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr createObj();

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern void deleteObj(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getBuffer(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getBufferSize(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getCategoriesBuffer(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getCategoriesBufferSize(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getMetricsBuffer(IntPtr instance);
    
    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getPropertiesIDBuffer(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getPropertiesIDBufferSize(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getPropertiesDescriptionBuffer(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getPropertiesDescriptionBufferSize(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getPropertiesValueBuffer(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getCategoriesCount(IntPtr instance);
    
    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getMetricsCount(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int getPropertiesCount(IntPtr instance);

    [DllImport("malioc-plugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int compileShader(IntPtr instance, string compiler, string shader, int stageIndex, int apiIndex, string gpuID);

    private IntPtr context;
    
    private IntPtr sourceBuffer;
    private int sourceBufferSize;
    private IntPtr categoriesBuffer;
    private int categoriesBufferSize;
    private IntPtr metricsBuffer;
    private IntPtr propertiesIDBuffer;
    private int propertiesIDBufferSize;
    private IntPtr propertiesDescriptionBuffer;
    private int propertiesDescriptionBufferSize;
    private IntPtr propertiesValueBuffer;

    private int categoriesCount;
    private int metricsCount;
    private int propertiesCount;

    private byte[] sourceBufferData;
    private byte[] categoriesData;
    private float[] metricsData;
    private byte[] propertiesIDData;
    private byte[] propertiesDescriptionData;
    private int[] propertiesValData;

    private string report;

    public MaliocPlugin(){
        context = createObj();

    }

    public int CompileShader(string compilerPath, string shaderPath, int stageIndex, int apiIndex, string gpuID){
        int ret = compileShader(context, compilerPath, shaderPath, stageIndex, apiIndex, gpuID); 
        return ret;
    }
    
    public void CopyDataToManagedBuffers()
    {
        sourceBuffer = getBuffer(context);
        sourceBufferSize = getBufferSize(context);
        metricsBuffer = getMetricsBuffer(context);
        categoriesBuffer = getCategoriesBuffer(context);
        categoriesBufferSize = getCategoriesBufferSize(context);

        propertiesIDBuffer = getPropertiesIDBuffer(context);
        propertiesIDBufferSize = getPropertiesIDBufferSize(context);
        propertiesDescriptionBuffer = getPropertiesDescriptionBuffer(context);
        propertiesDescriptionBufferSize = getPropertiesDescriptionBufferSize(context);
        propertiesValueBuffer = getPropertiesValueBuffer(context);

        categoriesCount = getCategoriesCount(context);
        metricsCount = getMetricsCount(context);
        propertiesCount = getPropertiesCount(context);

        sourceBufferData = new byte[sourceBufferSize];
        categoriesData = new byte[categoriesBufferSize];
        metricsData = new float[metricsCount * categoriesCount];  
        propertiesIDData = new byte[propertiesIDBufferSize];
        propertiesDescriptionData = new byte[propertiesDescriptionBufferSize];
        propertiesValData = new int[propertiesCount];

        Marshal.Copy(sourceBuffer, sourceBufferData, 0, sourceBufferSize);
        Marshal.Copy(metricsBuffer, metricsData, 0, metricsCount * categoriesCount);
        Marshal.Copy(categoriesBuffer, categoriesData, 0, categoriesBufferSize);
        Marshal.Copy(propertiesIDBuffer, propertiesIDData, 0, propertiesIDBufferSize);
        Marshal.Copy(propertiesDescriptionBuffer, propertiesDescriptionData, 0, propertiesDescriptionBufferSize);
        Marshal.Copy(propertiesValueBuffer, propertiesValData, 0, propertiesCount);
    }

    public ShaderPerfUtil.ShaderPerfReport GetReport(){
        ShaderPerfUtil.ShaderPerfReport perfReport = new ShaderPerfUtil.ShaderPerfReport();
        perfReport.timestamp = DateTime.Now.ToString("MM/dd/yyyy hh:mm");
        perfReport.categories = new string[categoriesCount];
        perfReport.colors = new Color[categoriesCount];

        perfReport.source = Encoding.UTF8.GetString(sourceBufferData, 0, sourceBufferData.Length);

        string categoriesString = Encoding.UTF8.GetString(categoriesData, 0, categoriesData.Length);
        string[] categoriesArray = categoriesString.Split("/n");

        float hueStep = 1.0f / (float)categoriesCount;
        for (int i=0; i<categoriesCount; i++){
            float hue = hueStep * i;
            float sat = 0.65f;
            float val = 0.5f;
            perfReport.categories[i] = categoriesArray[i];
            perfReport.colors[i] = Color.HSVToRGB(hue, sat, val);
        }

        perfReport.metrics = new float[metricsCount][];
        for (int i=0; i<metricsCount; i++){
            perfReport.metrics[i] = new float[categoriesCount];
            for (int j=0; j<categoriesCount; j++){
                int index = i * categoriesCount + j;
                perfReport.metrics[i][j] = metricsData[index];
            }
        }
        perfReport.metricNames = new string[3];
        perfReport.metricNames[0] = "Shortest Path Cycles";
        perfReport.metricNames[1] = "Longest Path Cycles";
        perfReport.metricNames[2] = "Total Cycles";

        perfReport.propsID = new string[propertiesCount];
        perfReport.propsDesc = new string[propertiesCount];
        perfReport.propsVal = new int[propertiesCount];

        string propIDString = Encoding.UTF8.GetString(propertiesIDData, 0, propertiesIDData.Length);
        string[] propIDArray = propIDString.Split("/n");

        string propDescString = Encoding.UTF8.GetString(propertiesDescriptionData, 0, propertiesDescriptionData.Length);
        string[] propDescArray = propDescString.Split("/n");

        for (int i=0; i<propertiesCount; i++){
            perfReport.propsID[i] = propIDArray[i];
            perfReport.propsDesc[i] = propDescArray[i];
            perfReport.propsVal[i] = propertiesValData[i];
        }

        return perfReport;
    }

}

public static class ShaderPerfUtil 
{
    public class ShaderPerfReport{
        public string timestamp;
        public string[] categories;
        public Color[] colors;
        public string[] metricNames;
        public float[][] metrics;
        public string[] propsID;
        public string[] propsDesc;
        public int[] propsVal;
        public string source;
    }

    public enum Compiler
    {
        MALIOC = 0
    }

    public static readonly string[] MALIOC_GPUS = {
        // Valhall architecture
        "Immortalis-G715",
        "Mali-G715", 
        "Mali-G710",
        "Mali-G615",
        "Mali-G610",
        "Mali-G510",
        "Mali-G310",
        "Mali-G78AE",
        "Mali-G78",
        "Mali-G77",
        "Mali-G68",
        "Mali-G57",
        //Bifrost architecture
        "Mali-G76",
        "Mali-G72",
        "Mali-G71",
        "Mali-G52",
        "Mali-G51",
        "Mali-G31",
        //Midgard architecture
        "Mali-T880",
        "Mali-T860",
        "Mali-T830",
        "Mali-T820",
        "Mali-T760",
        "Mali-T720",
    };

    private static MaliocPlugin maliocHandler;

    private static string compilerPath;
    private static string compiledShaderPath;

    private static byte[] compiledShader;
    private static int stageIndex;

    public struct ShaderDescription{
        public ShaderData.Pass pass;
        public ShaderType stage;
        public string[] keywords;
    }

    public struct CompilerDescription{
        public Compiler compiler;
        public ShaderCompilerPlatform api;
        public BuildTarget target; 
        public string gpuID;
    }

    public static int CompileAndReport(string compilerPath, ShaderDescription shaderDesc, CompilerDescription compilerDesc, out ShaderPerfReport report)
    {

        report = new ShaderPerfReport();

        string gpuName;
        string stageExt = null;
        switch (compilerDesc.compiler){
            case Compiler.MALIOC:
                maliocHandler = maliocHandler ?? new MaliocPlugin();
                gpuName = compilerDesc.gpuID;
                break;         
            default:
                Debug.Log("Offline Shader Analysis: unsupported shader compiler");
                return 0;
        }

        int apiIndex;
        switch (compilerDesc.api){
            case ShaderCompilerPlatform.GLES3x:
                apiIndex = 0;
                switch (shaderDesc.stage){
                    case ShaderType.Vertex:
                        stageExt = ".vert";
                        stageIndex = 0;
                        break;
                    case ShaderType.Fragment:
                        stageExt = ".frag";
                        stageIndex = 1;
                        break;
                    default:
                        Debug.Log("Offline Shader Analysis: unsupported shader stage");
                        return 0;
                }
                break;
            case ShaderCompilerPlatform.Vulkan:
                apiIndex = 1;
              switch (shaderDesc.stage){
                    case ShaderType.Vertex:
                        stageExt = ".vert.spv";
                        stageIndex = 0;
                        break;
                    case ShaderType.Fragment:
                        stageExt = ".frag.spv";
                        stageIndex = 1;
                        break;
                    default:
                        Debug.Log("Offline Shader Analysis: unsupported shader stage");
                        return 0;
                }
                break;
            default:
                Debug.Log("Offline Shader Analysis: Unsupported graphics API");
                return 0;
        }

        
        string tempShadersPath =  string.Format("{0}", Path.Combine("Assets/", "OfflineShaderAnalysis"));
        if (!Directory.Exists(tempShadersPath))
        {
            Directory.CreateDirectory(tempShadersPath);
        }
        string compiledShaderPath = string.Format("{0}", Path.Combine(tempShadersPath, "compiled_shader" + stageExt));

        ShaderData.VariantCompileInfo compileInfo = shaderDesc.pass.CompileVariant(shaderDesc.stage, shaderDesc.keywords, compilerDesc.api, compilerDesc.target, true);
        if (!compileInfo.Success){
            Debug.Log("Offline Shader Analysis: failed to compile shader");
            ShaderMessage[] warnings = compileInfo.Messages;
            for (int i=0; i<warnings.Length; i++){
                 Debug.Log(String.Format("OfflineShaderAnalysis: {0}", warnings[i].message));
            }
            return 0;
        }
        
        compiledShader = compileInfo.ShaderData;

        string source = Encoding.UTF8.GetString(compiledShader);
        int firstNewline = source.IndexOf('\n');
        int length = (firstNewline == -1) ? compiledShader.Length : firstNewline;
        string firstLine = Encoding.UTF8.GetString(compiledShader, 0, length).Trim();

        if ((firstNewline != -1) && (firstLine == "#version 300 es"))
        {
            string updatedSource = "#version 310 es" + source.Substring(firstNewline);
            compiledShader = Encoding.UTF8.GetBytes(updatedSource);
        }

        using (FileStream fs = File.Create(compiledShaderPath))
        {
            Debug.Log("Offline Shader Analysis: written compiled shader to " + compiledShaderPath);
            fs.Write(compiledShader); 
        }

        string assetFolderPath = Application.dataPath;
        string exportPath = string.Format("{0}", Path.Combine(assetFolderPath, "OfflineShaderAnalysis", "compiled_shader" + stageExt));

        if(string.IsNullOrWhiteSpace(compilerPath))
        {
            Debug.Log("Offline Shader Analysis: please specify a valid path pointing to the offline compiler!");
            return 0;
        }

        if (!File.Exists(compilerPath))
        {
            Debug.Log("Offline Shader Analysis: no file was found in the compiler path you specified!");
            return 0;
        } else
        {
            string fileName = Path.GetFileName(compilerPath);
            string expectedFileName = "";
            if (Application.platform == RuntimePlatform.WindowsEditor){
                expectedFileName = "malioc.exe";
            } else if (Application.platform == RuntimePlatform.OSXEditor){
                expectedFileName = "malioc";
            }

            if (!fileName.Equals(expectedFileName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("Offline Shader Analysis: valid offline compiler (MALIOC) could not be found at the provided path");
                return 0;
            }
        }

        int compilationStatus;
        if (Application.platform == RuntimePlatform.WindowsEditor){
            compilationStatus = maliocHandler.CompileShader("@\"" + compilerPath + " \"", exportPath, stageIndex, apiIndex, gpuName);
        } else if (Application.platform == RuntimePlatform.OSXEditor){
            compilationStatus = maliocHandler.CompileShader("\"" + compilerPath + "\"", exportPath, stageIndex, apiIndex, gpuName);
        } else {
            Debug.Log("Offline Shader Analysis: editor platform is not supported");
            compilationStatus = 0;
        }

        switch (compilationStatus){
            case 0:
                Debug.Log("Offline Shader Analysis: plugin failed to process shader...");
                Debug.Log("Offline Shader Analysis: please check compiler and shader settings (see Editor log for more info)");
                break;
            case 1:
                maliocHandler.CopyDataToManagedBuffers();
                report = maliocHandler.GetReport(); 
                if (compilerDesc.api == ShaderCompilerPlatform.GLES3x) {
                    report.source = Encoding.UTF8.GetString(compiledShader, 0, compiledShader.Length);
                }
                break;
        }
        return compilationStatus;
    }

}