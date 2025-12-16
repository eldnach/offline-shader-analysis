using System;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ShaderPerfEditor : EditorWindow
{
    // UI Templates
    static VisualTreeAsset EditorTemplate;
    static VisualTreeAsset KeywordTemplate;
    static VisualTreeAsset CategoryTemplate;
    static VisualTreeAsset MetricTemplate;
    static VisualTreeAsset BarTemplate;
    static VisualTreeAsset PropertyTemplate;
    static VisualTreeAsset ReportTemplate;

    // UI Elements
    VisualElement Editor;
    ObjectField ShaderField;
    DropdownField SubshaderDrop;
    DropdownField PassDrop;
    DropdownField StageDrop;
    GroupBox KeywordsList;
    Button KeywordsToggle;
    DropdownField CompilerDrop;
    TextField CompilerPath;
    Button CompilerPathDialog;
    DropdownField GPUDrop;
    DropdownField GraphicsAPIDrop;
    DropdownField BuildTargetDrop;
    Button CompileButton;
    ScrollView CompilerReports;

    // Properties
    List<string> _compilers;
    List<string> _gpus;
    List<string> _graphicsAPIs;
    List<string> _buildTargets;
    List<string> _subshaders;
    List<string> _passes;
    List<string> _stages;

    Shader _activeShader;
    ShaderData _activeShaderData;
    ShaderData.Subshader _activeSubshader;
    ShaderData.Pass _activePass;
    
    private class Keyword
    {
        public VisualElement element;
        public Button toggle;
        public TextField text; 
        public bool isEnabled = true;
        public string name;

        public Keyword(VisualElement _element, bool _state, string _name)
        {
            element = _element;
            isEnabled = _state;
            name = _name;

            toggle = element.Q<Button>("KeywordButton");
            toggle.text = ""; 
            toggle.style.backgroundColor = new StyleColor(new Color(0.1f, 0.4f, 0.1f)); // Dark Green for Enabled

            text = element.Q<TextField>("KeywordField");
            text.value = name;
            text.isReadOnly = true;

            toggle.RegisterCallback<MouseUpEvent, Keyword>(ToggleKeyword, this);
        }

        void ToggleKeyword(MouseUpEvent evt, Keyword keyword)
        {
            keyword.isEnabled = !keyword.isEnabled;
            if (keyword.isEnabled)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        // void OnTextFieldFocused(FocusInEvent evt, Keyword keyword)
        //{           
        //     keyword.ToggleKeyword(null, keyword); 
        //     keyword.text.Blur(); 
        //     evt.StopPropagation();
        // }

        public void Disable()
        {
            isEnabled = false; 
            toggle.style.backgroundColor = new StyleColor(new Color(0.5f, 0.1f, 0.1f)); // Dark Red for Disabled
            text.style.opacity = 0.5f; 
        }

        public void Enable()
        {
            isEnabled = true;
            toggle.text = ""; 
            toggle.style.backgroundColor = new StyleColor(new Color(0.1f, 0.4f, 0.1f)); // Dark Green for Enabled
            text.style.opacity = 1.0f; 
        }
    }
    List<Keyword> _activeKeywords;
    bool _keywordsOverride; 
    
    ShaderType _activeStage;
    ShaderCompilerPlatform _activeAPI;
    BuildTarget _activeTarget;
    ShaderPerfUtil.Compiler _activeCompiler;
    string _activeCompilerPath;
    string _activeGPU;

    [MenuItem("Window/Analysis/Offline Shader Analysis/Open _%#S")] // ctrl-shft-S
    public static void ShowWindow()
    {
        ShaderPerfEditor wnd = GetWindow<ShaderPerfEditor>();
        wnd.titleContent = new GUIContent("Offline Shader Analysis");
        wnd.minSize = new Vector2(800, 1000);
    }

    void LoadElements()
    {
        EditorTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Editor.uxml", typeof(VisualTreeAsset));
        KeywordTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Keyword.uxml", typeof(VisualTreeAsset));
        ReportTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/CompilerReport.uxml", typeof(VisualTreeAsset));
        CategoryTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Category.uxml", typeof(VisualTreeAsset));
        MetricTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Metric.uxml", typeof(VisualTreeAsset));
        BarTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Bar.uxml", typeof(VisualTreeAsset));
        PropertyTemplate = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/Resources/Property.uxml", typeof(VisualTreeAsset));
    }

    void QueryElements()
    {
        ShaderField = Editor.Q<ObjectField>("ShaderField");
        SubshaderDrop = Editor.Q<DropdownField>("SubshaderDrop");
        PassDrop = Editor.Q<DropdownField>("PassDrop");
        StageDrop = Editor.Q<DropdownField>("StageDrop");
        KeywordsList = Editor.Q<GroupBox>("KeywordList");

        KeywordsToggle = Editor.Q<Button>("KeywordsToggle");

        CompilerDrop = Editor.Q<DropdownField>("CompilerDrop");
        CompilerPath = Editor.Q<TextField>("CompilerPathField");
        CompilerPathDialog = Editor.Q<Button>("CompilerPathDialog");
        GPUDrop = Editor.Q<DropdownField>("GPUDrop");
        GraphicsAPIDrop = Editor.Q<DropdownField>("GraphicsAPIDrop");
        BuildTargetDrop = Editor.Q<DropdownField>("BuildTargetDrop");

        CompileButton = Editor.Q<Button>("CompileButton");

        CompilerReports = Editor.Q<ScrollView>("Reports");
    }

    void RegisterCallbacks(){
        ShaderField.RegisterValueChangedCallback(SelectShader);
        SubshaderDrop.RegisterValueChangedCallback(SelectSubshader);
        PassDrop.RegisterValueChangedCallback(SelectPass);
        StageDrop.RegisterValueChangedCallback(SelectStage);
        
        KeywordsToggle.RegisterCallback<MouseUpEvent, Button>(ToggleAllKeywords, KeywordsToggle);

        CompilerDrop.RegisterValueChangedCallback(SelectCompiler);
        CompilerPathDialog.RegisterCallback<MouseUpEvent, VisualElement>(SelectCompilerExecutable, CompilerPath);
        GPUDrop.RegisterValueChangedCallback(SelectGPU);
        GraphicsAPIDrop.RegisterValueChangedCallback(SelectGraphicsAPI);
        BuildTargetDrop.RegisterValueChangedCallback(SelectBuildTarget);

        CompileButton.RegisterCallback<MouseUpEvent>(CompileAndReport);

    }

    void SelectShader(ChangeEvent<UnityEngine.Object> evt){
        _activeShader = (Shader)ShaderField.value;
        InitializeShaderSettings();
    }

    void SelectCompilerExecutable(MouseUpEvent evt, VisualElement elm)
    {
        if(Application.platform == RuntimePlatform.WindowsEditor){
            CompilerPath.value = EditorUtility.OpenFilePanel("Select MALIOC executable", "", "exe");
        } else if (Application.platform == RuntimePlatform.OSXEditor){
            CompilerPath.value = EditorUtility.OpenFilePanel("Select MALIOC executable", "", "");
        }
    }

    void SelectSubshader(ChangeEvent<string> evt){
        _activeSubshader = _activeShaderData.GetSubshader(SubshaderDrop.index);
    }

    void SelectPass(ChangeEvent<string> evt){
        _activePass = _activeSubshader.GetPass(PassDrop.index);
    }

    void SelectStage(ChangeEvent<string> evt){
        _activeStage = (ShaderType)Enum.Parse(typeof(ShaderType), StageDrop.value);
    }

    void SelectCompiler(ChangeEvent<string> evt){
        _activeCompiler =  (ShaderPerfUtil.Compiler)CompilerDrop.index;
        UpdateCompilerSettings();
    }

    void SelectGPU(ChangeEvent<string> evt){
        _activeGPU =  (string)GPUDrop.value;
    }

    void SelectGraphicsAPI(ChangeEvent<string> evt){
        ShaderCompilerPlatform graphicsAPI; 
        Enum.TryParse<ShaderCompilerPlatform>(GraphicsAPIDrop.value, out graphicsAPI);
        _activeAPI = graphicsAPI;
    }

    void SelectBuildTarget(ChangeEvent<string> evt){
        BuildTarget buildTarget; 
        Enum.TryParse<BuildTarget>(BuildTargetDrop.value, out buildTarget);
        _activeTarget = buildTarget;
    }

    void RemoveReport(MouseUpEvent evt, VisualElement element){
        CompilerReports.Remove(element);
    }

    void ToggleAllKeywords(MouseUpEvent evt, Button button){

        if (_keywordsOverride)
        {
            DisableAllKeywords();
            _keywordsOverride = false; 
            button.text = "Enable All Keywords";
            button.style.backgroundColor = new StyleColor(new Color(0.1f, 0.4f, 0.1f));
            
        } else
        {
            EnableAllKeywords();
            _keywordsOverride = true;
            button.text = "Disable All Keywords";
            button.style.backgroundColor = new StyleColor(new Color(0.5f, 0.1f, 0.1f));
        }
    }

    void EnableAllKeywords(){
        foreach(Keyword key in _activeKeywords)
        {
            key.Enable();
        }
    }

    void DisableAllKeywords(){
        foreach(Keyword key in _activeKeywords)
        {
            key.Disable();
        }
    }

    void AppendKey(string name){
        VisualElement keywordElement = KeywordTemplate.Instantiate();
        Keyword keyword = new Keyword(keywordElement, true, name);
        _activeKeywords.Add(keyword);   
        KeywordsList.Add(keywordElement);
    }
    
    void QueryKeywords()
    {
        KeywordsList.Clear();
        _activeKeywords.Clear();
        LocalKeywordSpace keywordSpace = _activeShaderData.SourceShader.keywordSpace;
        foreach (LocalKeyword localKeyword in keywordSpace.keywords)
        {
            AppendKey(localKeyword.name);
        }
    }

    void InitializeCompilerSettings(){
        _compilers = new List<string>();
        ShaderPerfUtil.Compiler[] compilers = (ShaderPerfUtil.Compiler[])Enum.GetValues(typeof(ShaderPerfUtil.Compiler));
        for (int i=0; i<compilers.Length; i++){
            _compilers.Add(compilers[i].ToString());
        }
        CompilerDrop.choices = _compilers;
        CompilerDrop.index = 0;
        _activeCompiler =  ShaderPerfUtil.Compiler.MALIOC;
        string compilerName = _compilers[0].ToString();
        CompilerPath.value = String.Format("<Enter absolute path to {0} executable>", compilerName);

        UpdateCompilerSettings();
    }

    void UpdateCompilerSettings(){

        _gpus = new List<string>();
        _graphicsAPIs = new List<string>();
        _buildTargets = new List<string>();

        if (_activeCompiler == ShaderPerfUtil.Compiler.MALIOC){

            for (int i=0; i<ShaderPerfUtil.MALIOC_GPUS.Length; i++){
                _gpus.Add(ShaderPerfUtil.MALIOC_GPUS[i]);
            }
            _activeGPU = _gpus[0];
            _graphicsAPIs.Add(ShaderCompilerPlatform.GLES3x.ToString());
            _graphicsAPIs.Add(ShaderCompilerPlatform.Vulkan.ToString());
            _buildTargets.Add(BuildTarget.Android.ToString());
            _activeAPI = ShaderCompilerPlatform.GLES3x;
            _activeTarget = BuildTarget.Android;
        }
        GPUDrop.choices = _gpus;
        GPUDrop.index = 0;
        GraphicsAPIDrop.choices = _graphicsAPIs;
        GraphicsAPIDrop.index = 0;
        BuildTargetDrop.choices = _buildTargets;
        BuildTargetDrop.index = 0;
    }

    void LoadDefualtShader(){
        _activeShader = (Shader)AssetDatabase.LoadAssetAtPath("Packages/com.eldnach.offline-shader-analysis/Editor/TestShader.shader", typeof(Shader));
        ShaderField.objectType = typeof(UnityEngine.Shader);
        ShaderField.value = _activeShader;
    }

    void InitializeShaderSettings(){
        _subshaders = new List<string>();
        _passes = new List<string>();
        _stages = new List<string>();
        
        _activeKeywords = new List<Keyword>();
        _activeShaderData = ShaderUtil.GetShaderData(_activeShader);
        int subshaderCount = _activeShaderData.SubshaderCount;
        for (int i=0; i<subshaderCount; i++){
            _subshaders.Add(i.ToString());
        }
        SubshaderDrop.choices = _subshaders;
        if (_subshaders.Count > 0)
        {
            SubshaderDrop.index = 0;
            _activeSubshader = _activeShaderData.GetSubshader(0);
        }

        int passCount = _activeSubshader.PassCount;
        for (int i=0; i<passCount; i++){
            _passes.Add(i.ToString());
        }
        PassDrop.choices = _passes;
        if (_passes.Count > 0)
        {
            PassDrop.index = 0;
            _activePass = _activeSubshader.GetPass(0);
        }

        ShaderType[] stages = (ShaderType[])Enum.GetValues(typeof(ShaderType));
        for (int i=0; i<stages.Length; i++){
            if (_activePass.HasShaderStage(stages[i])){
                _stages.Add(stages[i].ToString());
            }
        }
        StageDrop.choices = _stages;
        if (_stages.Count > 1)
        {
            StageDrop.index = 1;
            _activeStage = ShaderType.Fragment;
        } 

        QueryKeywords();
    }

    void CompileAndReport(MouseUpEvent evt){

        List<string> enabledKeywords = new List<string>();

        foreach (var keyword in _activeKeywords)
        {
            if (keyword.isEnabled)
            {
                enabledKeywords.Add(keyword.name);
            }
        }

        ShaderPerfUtil.ShaderDescription shaderDesc = new ShaderPerfUtil.ShaderDescription{
            pass = _activePass,
            stage = _activeStage,
            keywords = enabledKeywords.ToArray(),
        };

        ShaderPerfUtil.CompilerDescription compilerDesc = new ShaderPerfUtil.CompilerDescription{
            compiler = _activeCompiler,
            api = _activeAPI,
            target = _activeTarget,
            gpuID = _activeGPU
        };

        ShaderPerfUtil.ShaderPerfReport report = new ShaderPerfUtil.ShaderPerfReport();

        int compilationStatus = ShaderPerfUtil.CompileAndReport(CompilerPath.value, shaderDesc, compilerDesc, out report);
        if (compilationStatus != 1) {
            return;
        }

        VisualElement reportInstance = ReportTemplate.Instantiate();
        VisualElement legend = reportInstance.Q<VisualElement>("Legend");
        VisualElement metrics = reportInstance.Q<VisualElement>("Metrics");

        Label source = reportInstance.Q<Label>("SourceText");
        source.text = report.source;

        for (int i=0; i<report.categories.Length; i++){
            VisualElement category = CategoryTemplate.Instantiate();
            Label categoryText = category.Q<Label>("Text");
            categoryText.text = report.categories[i];   
            VisualElement categoryIcon = category.Q<VisualElement>("Icon");
            categoryIcon.style.backgroundColor =  new StyleColor(report.colors[i]);
            legend.Add(category);
        }

        for (int i=0; i<report.metrics.Length; i++){

            VisualElement metric = MetricTemplate.Instantiate();
            Label metricText = metric.Q<Label>("MetricLabel");
            metricText.text = report.metricNames[i];  
            
            metrics.Add(metric);

            GroupBox bargraph = metric.Q<GroupBox>("BarGraph");

            float sum = 0.0f;
            for (int j=0; j<report.categories.Length; j++){
                sum += report.metrics[i][j];
            }

            for (int j=0; j<report.categories.Length; j++){
                VisualElement barInstance = BarTemplate.Instantiate();
                VisualElement bar = barInstance.Q<VisualElement>("Bar");
                bar.style.backgroundColor = new StyleColor(report.colors[j]);
                barInstance.style.flexGrow = 1;

                float val = report.metrics[i][j] / sum;
                Length len = Length.Percent(val * 100.0f);
                if(val>0.0f){
                    barInstance.style.minWidth = new UnityEngine.UIElements.StyleLength(len);
                    barInstance.style.maxWidth = new UnityEngine.UIElements.StyleLength(len);
                    Label barText = barInstance.Q<Label>("Label");
                    if (val > 0.05f){
                        barText.text = report.metrics[i][j].ToString("F2");
                        if (val <0.1f){
                            barText.style.scale = new Scale(new Vector3 (0.8f, 0.8f, 1.0f));
                        }
                    }
                    else {
                        barText.text = "";
                        barInstance.tooltip = report.metrics[i][j].ToString("F2");
                    }
                    bargraph.Add(barInstance);
                }
            }   
        
        }

        ScrollView scrollView = reportInstance.Q<ScrollView>("PropertyView");
        for (int i=0; i<report.propsID.Length; i++){
            VisualElement propertyInstance = PropertyTemplate.Instantiate();
            Label propID = propertyInstance.Q<Label>("PropertyID");
            
            if (report.propsID[i] == "Stack Alloca Size (bytes)" || report.propsID[i] == "Has uniform computation" || 
                report.propsID[i] == "16-bit Arithmetic Idle Lanes"){
                continue;
            } 

            if (report.propsID[i] == "Has Stack Spilling"){
                propID.text = "Register spilling";
            } else if (report.propsID[i] == "Stack Spill Size (bytes)"){
                propID.text = "Register spill size (bytes)";
            } 
            else
            {
                propID.text = report.propsID[i];     
            }
            propID.tooltip = report.propsDesc[i];
            Label propVal = propertyInstance.Q<Label>("PropertyValue");
            propVal.text = report.propsVal[i].ToString();
            propVal.tooltip = report.propsDesc[i];
            scrollView.Add(propertyInstance);
        
        }

        Button removeButton = reportInstance.Q<Button>("RemoveButton");
        Label title = reportInstance.Q<Label>("Text");
        string shaderName = _activeShader.ToString();
        string reportDate = report.timestamp;
        
        title.text = String.Format("{0} - {1} - {2}", shaderName, _activeGPU, reportDate);
        removeButton.RegisterCallback<MouseUpEvent, VisualElement>(RemoveReport, reportInstance);
        CompilerReports.Add(reportInstance);

    }

    public void CreateGUI()
    {
        // Initialization 
        LoadElements();

        Editor = EditorTemplate.Instantiate();
        var root = rootVisualElement;
        root.Add(Editor);

        QueryElements();

        InitializeCompilerSettings();
        LoadDefualtShader();
        InitializeShaderSettings();

        RegisterCallbacks();
    }


}