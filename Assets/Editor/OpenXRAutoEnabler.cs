using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenXRAutoSetup
{
    [InitializeOnLoad]
    public class OpenXRAutoEnabler
    {
        private static AddRequest addXRPluginRequest;
        private static AddRequest addOpenXRRequest;
        private static AddRequest addXRInteractionRequest;
        private static ListRequest listRequest;
        private static bool isProcessing = false;
        
        private const string XR_PLUGIN_MANAGEMENT = "com.unity.xr.management";
        private const string OPENXR_PACKAGE = "com.unity.xr.openxr";
        private const string XR_INTERACTION_TOOLKIT = "com.unity.xr.interaction.toolkit";
        private const string PREFS_KEY = "OpenXRAutoEnabler_Completed";

        static OpenXRAutoEnabler()
        {
            // 检查是否真的已经配置完成
            if (EditorPrefs.GetBool(PREFS_KEY, false))
            {
                // 简单检查XR设置是否存在（不调用Package Manager API避免锁定）
                if (IsOpenXRProperlyConfigured())
                {
                    Debug.Log("[OpenXR Auto Enabler] OpenXR 已经配置过，跳过自动配置");
                    return;
                }
                else
                {
                    Debug.LogWarning("[OpenXR Auto Enabler] 检测到配置标记但OpenXR未正确配置，重新开始配置...");
                    EditorPrefs.DeleteKey(PREFS_KEY);
                }
            }

            EditorApplication.update += Initialize;
        }
        
        private static bool IsOpenXRProperlyConfigured()
        {
            try
            {
                // 只检查XR设置，不调用Package Manager API以避免锁定问题
                // 检查OpenXR类型是否存在（说明包已加载）
                var openXRLoaderType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR");
                if (openXRLoaderType == null)
                {
                    return false;
                }
                
                // 检查是否已在XR设置中启用OpenXR
                var xrGeneralSettingsPerBuildTargetType = System.Type.GetType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget, Unity.XR.Management.Editor");
                if (xrGeneralSettingsPerBuildTargetType == null)
                {
                    return false;
                }
                
                var instanceProperty = xrGeneralSettingsPerBuildTargetType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        // 有实例，认为已配置
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            
            if (!isProcessing)
            {
                isProcessing = true;
                Debug.Log("[OpenXR Auto Enabler] 开始检查 OpenXR 环境...");
                CheckOpenXREnvironment();
            }
        }

        private static void CheckOpenXREnvironment()
        {
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            Debug.Log("[OpenXR Auto Enabler] 开始检查 OpenXR 运行时环境...");
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            
            var envStatus = GetOpenXREnvironmentStatus();
            
            // 显示检查结果
            Debug.Log($"[OpenXR Auto Enabler] OpenXR 运行时: {(envStatus.hasRuntime ? "✓ 已检测到" : "✗ 未检测到")}");
            
            if (envStatus.runtimeDetails.Count > 0)
            {
                foreach (var detail in envStatus.runtimeDetails)
                {
                    Debug.Log($"[OpenXR Auto Enabler]   - {detail}");
                }
            }
            
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            
            // 检查是否为AIXRAgent (Pimax Runtime)
            if (envStatus.hasRuntime && !envStatus.hasAIXRAgent)
            {
                Debug.LogWarning("[OpenXR Auto Enabler] 警告：检测到其他 OpenXR 运行时，但不是 AIXRAgent！");
                Debug.LogWarning("[OpenXR Auto Enabler] 当前项目需要 AIXRAgent。");
                
                EditorApplication.delayCall += () => 
                {
                    bool shouldInstall = EditorUtility.DisplayDialog(
                        "AIXRAgent 未安装",
                        "系统中未检测到 AIXRAgent。\n\n" +
                        "当前检测到其他 OpenXR 运行时，但本项目需要 AIXRAgent 才能正常运行。\n\n" +
                        "是否继续安装 Unity OpenXR 包？\n" +
                        "（您需要先安装 AIXRAgent）",
                        "继续安装包",
                        "取消"
                    );
                    
                    if (shouldInstall)
                    {
                        Debug.Log("[OpenXR Auto Enabler] 用户选择继续安装 Unity OpenXR 包...");
                        CheckAndInstallPackages();
                    }
                    else
                    {
                        Debug.Log("[OpenXR Auto Enabler] 用户取消安装");
                        isProcessing = false;
                    }
                };
                return;
            }
            
            // 如果没有检测到任何运行时，给出提示和安装建议
            if (!envStatus.hasRuntime)
            {
                Debug.LogWarning("[OpenXR Auto Enabler] 警告：未检测到 OpenXR 运行时！");
                Debug.LogWarning("[OpenXR Auto Enabler] 您需要安装 AIXRAgent。");
                
                EditorApplication.delayCall += () => 
                {
                    bool shouldInstall = EditorUtility.DisplayDialog(
                        "AIXRAgent 未安装",
                        "系统中未检测到 AIXRAgent。\n\n" +
                        "要运行本 OpenXR 应用，您需要安装 AIXRAgent。\n\n" +
                        "是否继续安装 Unity OpenXR 包？\n" +
                        "（您需要先安装 AIXRAgent）",
                        "继续安装包",
                        "取消"
                    );
                    
                    if (shouldInstall)
                    {
                        Debug.Log("[OpenXR Auto Enabler] 用户选择继续安装 Unity OpenXR 包...");
                        CheckAndInstallPackages();
                    }
                    else
                    {
                        Debug.Log("[OpenXR Auto Enabler] 用户取消安装");
                        isProcessing = false;
                    }
                };
                return;
            }
            
            // 有AIXRAgent运行时环境，检查进程是否运行
            Debug.Log("[OpenXR Auto Enabler] ✓ AIXRAgent检测通过");
            
            // 检查AIXR Agent进程是否运行
            if (!IsAIXRAgentRunning())
            {
                Debug.LogWarning("[OpenXR Auto Enabler] AIXR Agent.exe 未运行，尝试启动...");
                
                if (!string.IsNullOrEmpty(envStatus.aixrAgentPath))
                {
                    if (StartAIXRAgent(envStatus.aixrAgentPath))
                    {
                        Debug.Log("[OpenXR Auto Enabler] ✓ AIXR Agent.exe 已启动");
                    }
                    else
                    {
                        Debug.LogWarning("[OpenXR Auto Enabler] 无法启动 AIXR Agent.exe");
                    }
                }
                else
                {
                    Debug.LogWarning("[OpenXR Auto Enabler] 无法确定 AIXR Agent.exe 路径");
                }
            }
            else
            {
                Debug.Log("[OpenXR Auto Enabler] ✓ AIXR Agent.exe 正在运行");
            }
            
            Debug.Log("[OpenXR Auto Enabler] 开始安装 Unity 包...");
            CheckAndInstallPackages();
        }
        
        private class OpenXREnvironmentStatus
        {
            public bool hasRuntime;
            public bool hasAIXRAgent;
            public string aixrAgentPath;
            public List<string> runtimeDetails = new List<string>();
        }
        
        private static OpenXREnvironmentStatus GetOpenXREnvironmentStatus()
        {
            var status = new OpenXREnvironmentStatus();
            
            // 1. 检查 OpenXR 运行时注册表（Windows）
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                try
                {
                    // 检查 OpenXR 运行时注册
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Khronos\OpenXR\1"))
                    {
                        if (key != null)
                        {
                            var activeRuntime = key.GetValue("ActiveRuntime");
                            if (activeRuntime != null)
                            {
                                status.hasRuntime = true;
                                string runtimePath = activeRuntime.ToString();
                                status.runtimeDetails.Add($"活动运行时: {runtimePath}");
                                
                                // 检查是否是pimax-openxr.json
                                if (runtimePath.Contains("pimax-openxr.json"))
                                {
                                    status.hasAIXRAgent = true;
                                    status.aixrAgentPath = ExtractAIXRAgentPath(runtimePath);
                                    status.runtimeDetails.Add("✓ 检测到AIXRAgent");
                                    if (!string.IsNullOrEmpty(status.aixrAgentPath))
                                    {
                                        status.runtimeDetails.Add($"AIXR Agent 路径: {status.aixrAgentPath}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // 检查 WOW6432Node（32位应用在64位系统）
                    if (!status.hasRuntime)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Khronos\OpenXR\1"))
                        {
                            if (key != null)
                            {
                                var activeRuntime = key.GetValue("ActiveRuntime");
                                if (activeRuntime != null)
                                {
                                    status.hasRuntime = true;
                                    string runtimePath = activeRuntime.ToString();
                                    status.runtimeDetails.Add($"活动运行时 (WOW64): {runtimePath}");
                                    
                                    // 检查是否是pimax-openxr.json
                                    if (runtimePath.Contains("pimax-openxr.json"))
                                    {
                                        status.hasAIXRAgent = true;
                                        status.aixrAgentPath = ExtractAIXRAgentPath(runtimePath);
                                        status.runtimeDetails.Add("✓ 检测到AIXRAgent");
                                        if (!string.IsNullOrEmpty(status.aixrAgentPath))
                                        {
                                            status.runtimeDetails.Add($"AIXR Agent 路径: {status.aixrAgentPath}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // 检查可用运行时列表
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes"))
                    {
                        if (key != null)
                        {
                            var runtimeNames = key.GetValueNames();
                            if (runtimeNames != null && runtimeNames.Length > 0)
                            {
                                status.hasRuntime = true;
                                status.runtimeDetails.Add($"可用运行时数量: {runtimeNames.Length}");
                                
                                // 检查可用运行时中是否有pimax-openxr.json
                                foreach (var runtime in runtimeNames)
                                {
                                    if (runtime.Contains("pimax-openxr.json"))
                                    {
                                        status.hasAIXRAgent = true;
                                        status.runtimeDetails.Add($"✓ 发现 AIXRAgent 运行时: {runtime}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[OpenXR Auto Enabler] 检查注册表时出错: {e.Message}");
                }
            }
            
            return status;
        }
        
        private static string ExtractAIXRAgentPath(string openxrJsonPath)
        {
            try
            {
                // 从 pimax-openxr.json 路径提取 AIXR Agent 根目录
                // 例如: D:\Program Files\AIXR Agent\resource\tool\openxr_runtime\Release\pimax-openxr.json
                // 提取: D:\Program Files\AIXR Agent
                
                if (string.IsNullOrEmpty(openxrJsonPath))
                    return null;
                
                // 查找 "AIXR Agent" 目录
                int index = openxrJsonPath.IndexOf("AIXR Agent", System.StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    // 找到 "AIXR Agent" 后面的第一个路径分隔符
                    int endIndex = openxrJsonPath.IndexOf("\\", index + "AIXR Agent".Length);
                    if (endIndex < 0)
                        endIndex = openxrJsonPath.IndexOf("/", index + "AIXR Agent".Length);
                    
                    if (endIndex > 0)
                    {
                        return openxrJsonPath.Substring(0, endIndex);
                    }
                }
                
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 提取 AIXR Agent 路径时出错: {e.Message}");
                return null;
            }
        }
        
        private static bool IsAIXRAgentRunning()
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.ProcessName.Contains("AIXR Agent", System.StringComparison.OrdinalIgnoreCase) ||
                            process.ProcessName.Equals("AIXR Agent", System.StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // 忽略无法访问的进程
                    }
                }
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 检查 AIXR Agent 进程时出错: {e.Message}");
                return false;
            }
        }
        
        private static bool StartAIXRAgent(string aixrAgentRootPath)
        {
            try
            {
                string exePath = System.IO.Path.Combine(aixrAgentRootPath, "AIXR Agent.exe");
                
                if (!System.IO.File.Exists(exePath))
                {
                    Debug.LogWarning($"[OpenXR Auto Enabler] AIXR Agent.exe 不存在: {exePath}");
                    return false;
                }
                
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = aixrAgentRootPath,
                    UseShellExecute = true
                };
                
                System.Diagnostics.Process.Start(startInfo);
                Debug.Log($"[OpenXR Auto Enabler] 已启动: {exePath}");
                
                // 等待一小段时间让进程启动
                System.Threading.Thread.Sleep(1000);
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[OpenXR Auto Enabler] 启动 AIXR Agent 时出错: {e.Message}");
                return false;
            }
        }

        private static void CheckAndInstallPackages()
        {
            listRequest = Client.List();
            EditorApplication.update += CheckListProgress;
        }

        private static void CheckListProgress()
        {
            if (listRequest.IsCompleted)
            {
                EditorApplication.update -= CheckListProgress;

                if (listRequest.Status == StatusCode.Success)
                {
                    HashSet<string> installedPackages = new HashSet<string>();
                    foreach (var package in listRequest.Result)
                    {
                        installedPackages.Add(package.name);
                    }

                    bool needXRPlugin = !installedPackages.Contains(XR_PLUGIN_MANAGEMENT);
                    bool needOpenXR = !installedPackages.Contains(OPENXR_PACKAGE);
                    bool needXRInteraction = !installedPackages.Contains(XR_INTERACTION_TOOLKIT);

                    if (needXRPlugin)
                    {
                        Debug.Log($"[OpenXR Auto Enabler] 正在安装 {XR_PLUGIN_MANAGEMENT}...");
                        addXRPluginRequest = Client.Add(XR_PLUGIN_MANAGEMENT);
                        EditorApplication.update += CheckXRPluginProgress;
                    }
                    else if (needOpenXR)
                    {
                        Debug.Log($"[OpenXR Auto Enabler] XR Plugin Management 已安装，正在安装 {OPENXR_PACKAGE}...");
                        addOpenXRRequest = Client.Add(OPENXR_PACKAGE);
                        EditorApplication.update += CheckOpenXRProgress;
                    }
                    else if (needXRInteraction)
                    {
                        Debug.Log($"[OpenXR Auto Enabler] OpenXR 已安装，正在安装 {XR_INTERACTION_TOOLKIT}...");
                        addXRInteractionRequest = Client.Add(XR_INTERACTION_TOOLKIT);
                        EditorApplication.update += CheckXRInteractionProgress;
                    }
                    else
                    {
                        Debug.Log("[OpenXR Auto Enabler] 所有包已安装，正在配置 XR 设置...");
                        EditorApplication.delayCall += ConfigureXRSettings;
                    }
                }
                else
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 获取包列表失败: {listRequest.Error.message}");
                    isProcessing = false;
                }
            }
        }

        private static void CheckXRPluginProgress()
        {
            if (addXRPluginRequest.IsCompleted)
            {
                EditorApplication.update -= CheckXRPluginProgress;

                if (addXRPluginRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[OpenXR Auto Enabler] {XR_PLUGIN_MANAGEMENT} 安装成功！");
                    Debug.Log($"[OpenXR Auto Enabler] 正在安装 {OPENXR_PACKAGE}...");
                    addOpenXRRequest = Client.Add(OPENXR_PACKAGE);
                    EditorApplication.update += CheckOpenXRProgress;
                }
                else
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 安装 {XR_PLUGIN_MANAGEMENT} 失败: {addXRPluginRequest.Error.message}");
                    isProcessing = false;
                }
            }
        }

        private static void CheckOpenXRProgress()
        {
            if (addOpenXRRequest.IsCompleted)
            {
                EditorApplication.update -= CheckOpenXRProgress;

                if (addOpenXRRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[OpenXR Auto Enabler] {OPENXR_PACKAGE} 安装成功！");
                    Debug.Log($"[OpenXR Auto Enabler] 正在安装 {XR_INTERACTION_TOOLKIT}...");
                    addXRInteractionRequest = Client.Add(XR_INTERACTION_TOOLKIT);
                    EditorApplication.update += CheckXRInteractionProgress;
                }
                else
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 安装 {OPENXR_PACKAGE} 失败: {addOpenXRRequest.Error.message}");
                    isProcessing = false;
                }
            }
        }

        private static void CheckXRInteractionProgress()
        {
            if (addXRInteractionRequest.IsCompleted)
            {
                EditorApplication.update -= CheckXRInteractionProgress;

                if (addXRInteractionRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[OpenXR Auto Enabler] {XR_INTERACTION_TOOLKIT} 安装成功！");
                    Debug.Log("[OpenXR Auto Enabler] 正在配置 XR 设置...");
                    EditorApplication.delayCall += ConfigureXRSettings;
                }
                else
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 安装 {XR_INTERACTION_TOOLKIT} 失败: {addXRInteractionRequest.Error.message}");
                    Debug.Log("[OpenXR Auto Enabler] 继续配置 XR 设置...");
                    EditorApplication.delayCall += ConfigureXRSettings;
                }
            }
        }

        private static void ConfigureXRSettings()
        {
            try
            {
                Debug.Log("[OpenXR Auto Enabler] 开始配置 XR 设置...");
                
                // 等待一帧，确保包已完全加载
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        var xrGeneralSettingsPerBuildTargetType = System.Type.GetType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget, Unity.XR.Management.Editor");
                        var xrPackageMetadataStoreType = System.Type.GetType("UnityEditor.XR.Management.Metadata.XRPackageMetadataStore, Unity.XR.Management.Editor");
                        
                        if (xrGeneralSettingsPerBuildTargetType != null && xrPackageMetadataStoreType != null)
                        {
                            // 获取或创建XRGeneralSettings实例
                            var xrGeneralSettingsInstance = GetOrCreateXRGeneralSettings(xrGeneralSettingsPerBuildTargetType);
                            
                            bool standaloneSuccess = EnableOpenXRForBuildTarget(xrGeneralSettingsPerBuildTargetType, 
                                xrPackageMetadataStoreType, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                            
                            if (standaloneSuccess)
                            {
                                ConfigureOpenXRSettings();
                                
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                                
                                Debug.Log("[OpenXR Auto Enabler] ✓ XR 设置配置完成！");
                                
                                EditorPrefs.SetBool(PREFS_KEY, true);
                                isProcessing = false;
                                
                                EditorApplication.delayCall += () =>
                                {
                                    bool openSettings = EditorUtility.DisplayDialog(
                                        "OpenXR 配置完成",
                                        "OpenXR、XR Interaction Toolkit 等包已成功安装！\n\n" +
                                        "✓ OpenXR 已自动启用 (Standalone & Android)\n" +
                                        "✓ Stereo Rendering Mode: Multi Pass\n" +
                                        "✓ Depth Submission Mode: Depth 16-bit\n" +
                                        "✓ 交互配置文件已添加\n\n" +
                                        "是否打开 XR 设置查看详情？",
                                        "打开设置",
                                        "稍后"
                                    );

                                    if (openSettings)
                                    {
                                        SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                                    }
                                };
                                return;
                            }
                        }
                        
                        Debug.LogWarning("[OpenXR Auto Enabler] 无法自动配置，请手动设置");
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        isProcessing = false;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[OpenXR Auto Enabler] 配置失败: {e.Message}\n{e.StackTrace}");
                        EditorPrefs.SetBool(PREFS_KEY, true);
                        isProcessing = false;
                    }
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[OpenXR Auto Enabler] 配置失败: {e.Message}\n{e.StackTrace}");
                EditorPrefs.SetBool(PREFS_KEY, true);
                isProcessing = false;
            }
        }

        private static object GetOrCreateXRGeneralSettings(System.Type xrGeneralSettingsPerBuildTargetType)
        {
            try
            {
                // 尝试获取现有实例
                var xrGeneralSettingsProperty = xrGeneralSettingsPerBuildTargetType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                
                if (xrGeneralSettingsProperty != null)
                {
                    var instance = xrGeneralSettingsProperty.GetValue(null);
                    if (instance == null)
                    {
                        // 如果不存在，创建新实例
                        var getOrCreateMethod = xrGeneralSettingsPerBuildTargetType.GetMethod("GetOrCreate",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        
                        if (getOrCreateMethod != null)
                        {
                            instance = getOrCreateMethod.Invoke(null, null);
                            Debug.Log("[OpenXR Auto Enabler] 创建了新的 XRGeneralSettings 实例");
                        }
                    }
                    return instance;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 获取/创建 XRGeneralSettings 时出错: {e.Message}");
            }
            return null;
        }

        // 按照官方文档: https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/manual/EndUser.html
        private static bool EnableOpenXRForBuildTarget(
            System.Type xrGeneralSettingsPerBuildTargetType,
            System.Type xrPackageMetadataStoreType,
            BuildTargetGroup buildTargetGroup,
            BuildTarget buildTarget)
        {
            try
            {
                string platformName = buildTargetGroup.ToString();
                Debug.Log($"[OpenXR Auto Enabler] 正在为 {platformName} 配置 OpenXR...");
                
                // 步骤1: 获取或创建XRGeneralSettingsPerBuildTarget实例
                object xrGeneralSettingsPerBuildTargetInstance = null;
                
                // 首先尝试从AssetDatabase查找现有实例
                var existingAssets = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
                if (existingAssets.Length > 0)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(existingAssets[0]);
                    xrGeneralSettingsPerBuildTargetInstance = AssetDatabase.LoadAssetAtPath(assetPath, xrGeneralSettingsPerBuildTargetType);
                    Debug.Log($"[OpenXR Auto Enabler]   从资源加载了 XRGeneralSettingsPerBuildTarget 实例: {assetPath}");
                }
                
                // 如果没有找到，尝试通过反射获取或创建
                if (xrGeneralSettingsPerBuildTargetInstance == null)
                {
                    var xrGeneralSettingsInstanceProperty = xrGeneralSettingsPerBuildTargetType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    
                    if (xrGeneralSettingsInstanceProperty != null)
                    {
                        xrGeneralSettingsPerBuildTargetInstance = xrGeneralSettingsInstanceProperty.GetValue(null);
                    }
                    
                    if (xrGeneralSettingsPerBuildTargetInstance == null)
                    {
                        // 尝试使用GetOrCreate方法
                        var getOrCreateMethod = xrGeneralSettingsPerBuildTargetType.GetMethod("GetOrCreate",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (getOrCreateMethod != null)
                        {
                            xrGeneralSettingsPerBuildTargetInstance = getOrCreateMethod.Invoke(null, null);
                            Debug.Log($"[OpenXR Auto Enabler]   通过GetOrCreate创建了实例");
                        }
                        else
                        {
                            // 尝试直接创建ScriptableObject
                            xrGeneralSettingsPerBuildTargetInstance = ScriptableObject.CreateInstance(xrGeneralSettingsPerBuildTargetType);
                            if (xrGeneralSettingsPerBuildTargetInstance != null)
                            {
                                // 保存到Assets目录
                                var settingsPath = "Assets/XR/Settings";
                                if (!System.IO.Directory.Exists(settingsPath))
                                {
                                    System.IO.Directory.CreateDirectory(settingsPath);
                                }
                                var assetPath = $"{settingsPath}/XRGeneralSettings.asset";
                                AssetDatabase.CreateAsset(xrGeneralSettingsPerBuildTargetInstance as UnityEngine.Object, assetPath);
                                AssetDatabase.SaveAssets();
                                Debug.Log($"[OpenXR Auto Enabler]   创建并保存了新的实例: {assetPath}");
                            }
                        }
                    }
                }
                
                if (xrGeneralSettingsPerBuildTargetInstance == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法获取或创建 XRGeneralSettingsPerBuildTarget 实例");
                    return false;
                }
                
                // 步骤2: 获取指定平台的XRGeneralSettings
                var settingsForBuildTargetMethod = xrGeneralSettingsPerBuildTargetType.GetMethod(
                    "SettingsForBuildTarget",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (settingsForBuildTargetMethod == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法找到 SettingsForBuildTarget 方法");
                    return false;
                }
                
                var xrGeneralSettings = settingsForBuildTargetMethod.Invoke(
                    xrGeneralSettingsPerBuildTargetInstance, 
                    new object[] { buildTargetGroup });
                
                if (xrGeneralSettings == null)
                {
                    Debug.Log($"[OpenXR Auto Enabler]   {platformName} 的 XRGeneralSettings 不存在，创建中...");
                    
                    // 创建XRGeneralSettings
                    var xrGeneralSettingsType = System.Type.GetType("UnityEditor.XR.Management.XRGeneralSettings, Unity.XR.Management.Editor");
                    if (xrGeneralSettingsType != null)
                    {
                        xrGeneralSettings = ScriptableObject.CreateInstance(xrGeneralSettingsType);
                        if (xrGeneralSettings != null)
                        {
                            (xrGeneralSettings as UnityEngine.Object).name = $"XRGeneralSettings_{platformName}";
                            
                            // 设置到buildTarget
                            var setSettingsMethod = xrGeneralSettingsPerBuildTargetType.GetMethod(
                                "SetSettingsForBuildTarget",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            
                            if (setSettingsMethod != null)
                            {
                                setSettingsMethod.Invoke(
                                    xrGeneralSettingsPerBuildTargetInstance,
                                    new object[] { buildTargetGroup, xrGeneralSettings });
                                EditorUtility.SetDirty(xrGeneralSettingsPerBuildTargetInstance as UnityEngine.Object);
                            }
                        }
                    }
                }
                
                if (xrGeneralSettings == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法获取或创建 {platformName} 的 XRGeneralSettings");
                    return false;
                }
                
                // 步骤3: 获取或创建XRManagerSettings (pluginsSettings)
                var assignedSettingsProperty = xrGeneralSettings.GetType().GetProperty("AssignedSettings",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (assignedSettingsProperty == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法找到 AssignedSettings 属性");
                    return false;
                }
                
                var pluginsSettings = assignedSettingsProperty.GetValue(xrGeneralSettings);
                
                if (pluginsSettings == null)
                {
                    Debug.Log($"[OpenXR Auto Enabler]   创建 XRManagerSettings for {platformName}");
                    
                    var xrManagerSettingsType = System.Type.GetType("UnityEngine.XR.Management.XRManagerSettings, Unity.XR.Management");
                    if (xrManagerSettingsType != null)
                    {
                        pluginsSettings = ScriptableObject.CreateInstance(xrManagerSettingsType);
                        if (pluginsSettings != null)
                        {
                            (pluginsSettings as UnityEngine.Object).name = $"XRManagerSettings_{platformName}";
                            assignedSettingsProperty.SetValue(xrGeneralSettings, pluginsSettings);
                            
                            // 初始化loaders列表
                            var initializeLoaderMethod = xrManagerSettingsType.GetMethod("InitializeLoaderSync",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            
                            EditorUtility.SetDirty(pluginsSettings as UnityEngine.Object);
                            EditorUtility.SetDirty(xrGeneralSettings as UnityEngine.Object);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                
                if (pluginsSettings == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法创建 pluginsSettings");
                    return false;
                }
                
                // 步骤4: 使用 XRPackageMetadataStore.AssignLoader 添加OpenXR Loader
                var assignLoaderMethod = xrPackageMetadataStoreType.GetMethod("AssignLoader",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                
                if (assignLoaderMethod == null)
                {
                    Debug.LogError($"[OpenXR Auto Enabler] 无法找到 AssignLoader 方法");
                    return false;
                }
                
                string loaderTypeName = "UnityEngine.XR.OpenXR.OpenXRLoader";
                Debug.Log($"[OpenXR Auto Enabler]   调用 AssignLoader for {platformName}, Loader: {loaderTypeName}");
                
                // 先检查是否已经存在
                bool alreadyExists = false;
                var activeLoadersProperty = pluginsSettings.GetType().GetProperty("activeLoaders",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (activeLoadersProperty != null)
                {
                    var loaders = activeLoadersProperty.GetValue(pluginsSettings) as System.Collections.IList;
                    if (loaders != null)
                    {
                        foreach (var loader in loaders)
                        {
                            if (loader != null && loader.GetType().FullName == loaderTypeName)
                            {
                                alreadyExists = true;
                                Debug.Log($"[OpenXR Auto Enabler]   ✓ {platformName} OpenXR Loader 已存在");
                                break;
                            }
                        }
                    }
                }
                
                bool didAssign = false;
                if (!alreadyExists)
                {
                    try
                    {
                        var result = assignLoaderMethod.Invoke(null, new object[] { pluginsSettings, loaderTypeName, buildTargetGroup });
                        didAssign = result is bool && (bool)result;
                        
                        if (didAssign)
                        {
                            Debug.Log($"[OpenXR Auto Enabler]   ✓ {platformName} OpenXR Loader 已成功添加");
                        }
                        else
                        {
                            Debug.LogWarning($"[OpenXR Auto Enabler]   AssignLoader 返回 false for {platformName}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[OpenXR Auto Enabler]   AssignLoader 出错: {ex.Message}");
                    }
                }
                else
                {
                    didAssign = true; // 已存在视为成功
                }
                
                if (didAssign || alreadyExists)
                {
                    if (!alreadyExists)
                    {
                        Debug.Log($"[OpenXR Auto Enabler]   ✓ {platformName} OpenXR 已启用");
                    }
                    
                    // 设置自动加载和运行
                    var automaticLoadingProperty = pluginsSettings.GetType().GetProperty("automaticLoading",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (automaticLoadingProperty != null && automaticLoadingProperty.CanWrite)
                    {
                        automaticLoadingProperty.SetValue(pluginsSettings, true);
                        Debug.Log($"[OpenXR Auto Enabler]   ✓ {platformName} 自动加载已启用");
                    }
                    
                    var automaticRunningProperty = pluginsSettings.GetType().GetProperty("automaticRunning",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (automaticRunningProperty != null && automaticRunningProperty.CanWrite)
                    {
                        automaticRunningProperty.SetValue(pluginsSettings, true);
                        Debug.Log($"[OpenXR Auto Enabler]   ✓ {platformName} 自动运行已启用");
                    }
                    
                    // 强制标记为脏，确保保存
                    EditorUtility.SetDirty(pluginsSettings as UnityEngine.Object);
                    EditorUtility.SetDirty(xrGeneralSettings as UnityEngine.Object);
                    
                    // 立即保存资产
                    AssetDatabase.SaveAssets();
                    
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[OpenXR Auto Enabler] AssignLoader 返回 false for {platformName}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[OpenXR Auto Enabler] 配置 {buildTargetGroup} 时出错: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private static void ConfigureOpenXRSettings()
        {
            try
            {
                Debug.Log("[OpenXR Auto Enabler] 正在配置 OpenXR 详细设置...");
                
                var openXRSettingsType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRSettings, Unity.XR.OpenXR");
                if (openXRSettingsType == null)
                {
                    Debug.LogWarning("[OpenXR Auto Enabler] 无法找到 OpenXRSettings 类型");
                    return;
                }
                
                var settingsAssets = AssetDatabase.FindAssets("t:OpenXRSettings");
                
                if (settingsAssets.Length == 0)
                {
                    Debug.LogWarning("[OpenXR Auto Enabler] 未找到 OpenXRSettings 资源");
                    
                    var getSettingsMethod = openXRSettingsType.GetMethod("GetSettingsForBuildTargetGroup",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    
                    if (getSettingsMethod != null)
                    {
                        var standaloneSettings = getSettingsMethod.Invoke(null, new object[] { BuildTargetGroup.Standalone });
                        if (standaloneSettings != null)
                        {
                            ConfigureOpenXRSettingsInstance(standaloneSettings, "Standalone");
                        }
                        
                        var androidSettings = getSettingsMethod.Invoke(null, new object[] { BuildTargetGroup.Android });
                        if (androidSettings != null)
                        {
                            ConfigureOpenXRSettingsInstance(androidSettings, "Android");
                        }
                    }
                }
                else
                {
                    foreach (var guid in settingsAssets)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var settings = AssetDatabase.LoadAssetAtPath(path, openXRSettingsType);
                        
                        if (settings != null)
                        {
                            var platformName = path.Contains("Android") ? "Android" : "Standalone";
                            ConfigureOpenXRSettingsInstance(settings, platformName);
                        }
                    }
                }
                
                Debug.Log("[OpenXR Auto Enabler] ✓ OpenXR 详细设置配置完成");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 配置详细设置时出错: {e.Message}");
            }
        }

        private static void ConfigureOpenXRSettingsInstance(object settings, string platformName)
        {
            try
            {
                var settingsType = settings.GetType();
                bool changed = false;
                
                // 配置 Stereo Rendering Mode = Multi Pass
                var renderModeProperty = settingsType.GetProperty("renderMode",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (renderModeProperty != null)
                {
                    var renderModeType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRSettings+RenderMode, Unity.XR.OpenXR");
                    if (renderModeType != null)
                    {
                        var multiPassValue = System.Enum.ToObject(renderModeType, 0);
                        renderModeProperty.SetValue(settings, multiPassValue);
                        changed = true;
                        Debug.Log($"[OpenXR Auto Enabler]   {platformName}: Stereo Rendering = Multi Pass");
                    }
                }
                
                // 配置 Depth Submission Mode = Depth 16-bit
                var depthModeProperty = settingsType.GetProperty("depthSubmissionMode",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (depthModeProperty != null)
                {
                    var depthModeType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRSettings+DepthSubmissionMode, Unity.XR.OpenXR");
                    if (depthModeType != null)
                    {
                        var depth16BitValue = System.Enum.ToObject(depthModeType, 1);
                        depthModeProperty.SetValue(settings, depth16BitValue);
                        changed = true;
                        Debug.Log($"[OpenXR Auto Enabler]   {platformName}: Depth = 16-bit");
                    }
                }
                
                // 配置交互配置文件
                if (AddInteractionProfiles(settings, settingsType))
                {
                    changed = true;
                    Debug.Log($"[OpenXR Auto Enabler]   {platformName}: 交互配置文件已添加");
                }
                
                if (changed)
                {
                    EditorUtility.SetDirty(settings as UnityEngine.Object);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 配置 {platformName} 设置时出错: {e.Message}");
            }
        }

        private static bool AddInteractionProfiles(object settings, System.Type settingsType)
        {
            try
            {
                string[] profileTypeNames = new string[]
                {
                    "UnityEngine.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile, Unity.XR.OpenXR",
                    "UnityEngine.XR.OpenXR.Features.Interactions.HTCViveControllerProfile, Unity.XR.OpenXR",
                    "UnityEngine.XR.OpenXR.Features.Interactions.ValveIndexControllerProfile, Unity.XR.OpenXR"
                };
                
                string[] profileNames = new string[]
                {
                    "Oculus Touch",
                    "HTC Vive",
                    "Valve Index"
                };
                
                bool anyAdded = false;
                
                for (int i = 0; i < profileTypeNames.Length; i++)
                {
                    var profileType = System.Type.GetType(profileTypeNames[i]);
                    
                    if (profileType != null)
                    {
                        var getFeatureMethod = settingsType.GetMethod("GetFeature",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        
                        if (getFeatureMethod != null)
                        {
                            var genericMethod = getFeatureMethod.MakeGenericMethod(profileType);
                            var existingFeature = genericMethod.Invoke(settings, null);
                            
                            if (existingFeature != null)
                            {
                                var enabledProperty = profileType.GetProperty("enabled",
                                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                                
                                if (enabledProperty != null)
                                {
                                    var currentEnabled = (bool)enabledProperty.GetValue(existingFeature);
                                    if (!currentEnabled)
                                    {
                                        enabledProperty.SetValue(existingFeature, true);
                                        EditorUtility.SetDirty(existingFeature as UnityEngine.Object);
                                        anyAdded = true;
                                        Debug.Log($"[OpenXR Auto Enabler]     ✓ {profileNames[i]} 已启用");
                                    }
                                }
                            }
                        }
                    }
                }
                
                return anyAdded;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[OpenXR Auto Enabler] 添加交互配置文件时出错: {e.Message}");
                return false;
            }
        }

        [MenuItem("Tools/OpenXR/重置自动启用器")]
        public static void ResetAutoEnabler()
        {
            EditorPrefs.DeleteKey(PREFS_KEY);
            Debug.Log("[OpenXR Auto Enabler] 已重置，重启编辑器后将重新执行");
        }

        [MenuItem("Tools/OpenXR/手动触发 OpenXR 安装")]
        public static void ManualTriggerInstall()
        {
            if (isProcessing)
            {
                Debug.LogWarning("[OpenXR Auto Enabler] 安装正在进行中...");
                return;
            }

            EditorPrefs.DeleteKey(PREFS_KEY);
            isProcessing = true;
            Debug.Log("[OpenXR Auto Enabler] 手动触发安装...");
            CheckOpenXREnvironment();
        }
        
        [MenuItem("Tools/OpenXR/检查 OpenXR 环境")]
        public static void CheckEnvironmentOnly()
        {
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            Debug.Log("[OpenXR Auto Enabler] OpenXR 环境检查报告");
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            
            var envStatus = GetOpenXREnvironmentStatus();
            
            Debug.Log($"[OpenXR Auto Enabler] OpenXR 运行时: {(envStatus.hasRuntime ? "✓ 已检测到" : "✗ 未检测到")}");
            
            if (envStatus.runtimeDetails.Count > 0)
            {
                foreach (var detail in envStatus.runtimeDetails)
                {
                    Debug.Log($"[OpenXR Auto Enabler]   {detail}");
                }
            }
            
            Debug.Log("[OpenXR Auto Enabler] ==================================================");
            
            if (!envStatus.hasRuntime)
            {
                Debug.LogWarning("[OpenXR Auto Enabler] 建议：请安装 AIXRAgent");
                
                EditorUtility.DisplayDialog(
                    "AIXRAgent 环境检查",
                    "未检测到 AIXRAgent！\n\n" +
                    "要运行本 OpenXR 应用，请安装 AIXRAgent。\n\n" +
                    "详细信息请查看 Console 日志。",
                    "确定"
                );
            }
            else if (!envStatus.hasAIXRAgent)
            {
                Debug.LogWarning("[OpenXR Auto Enabler] 警告：检测到其他 OpenXR 运行时，但不是 AIXRAgent");
                
                EditorUtility.DisplayDialog(
                    "AIXRAgent 环境检查",
                    "⚠ 检测到其他 OpenXR 运行时，但不是 AIXRAgent！\n\n" +
                    "当前项目需要 AIXRAgent 才能正常运行。\n\n" +
                    "详细信息请查看 Console 日志。",
                    "确定"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "AIXRAgent 环境检查",
                    "✓ AIXRAgent 已正确安装！\n\n" +
                    "详细信息请查看 Console 日志。",
                    "确定"
                );
            }
        }

        [MenuItem("Tools/OpenXR/打开 XR 设置")]
        public static void OpenXRSettings()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
    }
}
