using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

//https://stackoverflow.com/questions/43293173/use-custom-manifest-file-and-permission-in-unity
public class ModifyUnityAndroidAppManifestSample : IPostGenerateGradleAndroidProject
{
    
    public void OnPostGenerateGradleAndroidProject(string basePath)
    {
        // If needed, add condition checks on whether you need to run the modification routine.
        // For example, specific configuration/app options enabled
        string manifestPath = GetManifestPath(basePath);
        var androidManifest = new AndroidManifest(manifestPath);

        androidManifest.SetUSBHostFeature();
        androidManifest.SetUSBIntents();
        androidManifest.SetUSBMetadata();
        androidManifest.SetExtStoragePermission();
        androidManifest.SetMGDPermission();
        MergeResources(basePath);

        // Add your XML manipulation routines

        androidManifest.Save();
    }

    public int callbackOrder { get { return 1; } }

    private string _manifestFilePath;

    private string GetManifestPath(string basePath)
    {
        if (string.IsNullOrEmpty(_manifestFilePath))
        {
            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();
        }
        return _manifestFilePath;
    }

    private void MergeResources(string basePath)
    {
        var pathToRes = new StringBuilder(Application.dataPath); //+ "/Plugins/Android/res";
        pathToRes.Append(Path.DirectorySeparatorChar).Append("Plugins");
        pathToRes.Append(Path.DirectorySeparatorChar).Append("Android");
        pathToRes.Append(Path.DirectorySeparatorChar).Append("res");

        var pathBuilder = new StringBuilder(basePath);
        pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
        pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
        pathBuilder.Append(Path.DirectorySeparatorChar).Append("res");

        Copy(pathToRes.ToString(), pathBuilder.ToString());
    }

    public static void Copy(string sourceDirectory, string targetDirectory)
    {
        var diSource = new DirectoryInfo(sourceDirectory);
        var diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Name.EndsWith("meta"))
                continue;
            Debug.Log($"Copying " + target.FullName +" to " + fi.Name);
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}


internal class AndroidXmlDocument : XmlDocument
{
    private string m_Path;
    protected XmlNamespaceManager nsMgr;
    public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
    public AndroidXmlDocument(string path)
    {
        m_Path = path;
        using (var reader = new XmlTextReader(m_Path))
        {
            reader.Read();
            Load(reader);
        }
        nsMgr = new XmlNamespaceManager(NameTable);
        nsMgr.AddNamespace("android", AndroidXmlNamespace);
    }

    public string Save()
    {
        return SaveAs(m_Path);
    }

    public string SaveAs(string path)
    {
        using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
        {
            writer.Formatting = Formatting.Indented;
            Save(writer);
        }
        return path;
    }
}


internal class AndroidManifest : AndroidXmlDocument
{
    private readonly XmlElement ApplicationElement;

    public AndroidManifest(string path) : base(path)
    {
        ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
    }

    private XmlAttribute CreateAndroidAttribute(string key, string value)
    {
        XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
        attr.Value = value;
        return attr;
    }

    internal XmlNode GetActivityWithLaunchIntent()
    {
        return SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nsMgr);
    }

    internal void SetApplicationTheme(string appTheme)
    {
        ApplicationElement.Attributes.Append(CreateAndroidAttribute("theme", appTheme));
    }

    internal void SetStartingActivityName(string activityName)
    {
        GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("name", activityName));
    }


    internal void SetHardwareAcceleration()
    {
        GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("hardwareAccelerated", "true"));
    }

    internal void SetMicrophonePermission()
    {
        var manifest = SelectSingleNode("/manifest");
        XmlElement child = CreateElement("uses-permission");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.RECORD_AUDIO");
        child.Attributes.Append(newAttribute);
    }

    internal void SetExtStoragePermission()
    {
        var manifest = SelectSingleNode("/manifest");
        XmlElement child = CreateElement("uses-permission");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.READ_EXTERNAL_STORAGE");
        child.Attributes.Append(newAttribute);
    }

    internal void SetMGDPermission()
    {
        var manifest = SelectSingleNode("/manifest");
        XmlElement child = CreateElement("uses-permission");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.MANAGE_DOCUMENTS");
        child.Attributes.Append(newAttribute);
    }

    internal void SetUSBHostFeature()
    {
        var manifest = SelectSingleNode("/manifest");
        XmlElement child = CreateElement("uses-feature");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.hardware.usb.host");
        child.Attributes.Append(newAttribute);
    }

    internal void SetUSBIntents()
    {
        var manifest = SelectSingleNode("/manifest/application/activity/intent-filter");
        XmlElement child = CreateElement("action");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.hardware.usb.action.USB_DEVICE_ATTACHED");
        child.Attributes.Append(newAttribute);
        manifest = SelectSingleNode("/manifest/application/activity/intent-filter");
        child = CreateElement("action");
        manifest.AppendChild(child);
        newAttribute = CreateAndroidAttribute("name", "android.hardware.usb.action.USB_DEVICE_DETACHED");
        child.Attributes.Append(newAttribute);
    }

    internal void SetUSBMetadata()
    {
        var manifest = SelectSingleNode("/manifest/application/activity");
        XmlElement child = CreateElement("meta-data");
        manifest.AppendChild(child);
        XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.hardware.usb.action.USB_DEVICE_ATTACHED");
        child.Attributes.Append(newAttribute);
        newAttribute = CreateAndroidAttribute("resource", "@xml/device_filter");
        child.Attributes.Append(newAttribute);
        manifest = SelectSingleNode("/manifest/application/activity");
        child = CreateElement("meta-data");
        manifest.AppendChild(child);
        newAttribute = CreateAndroidAttribute("name", "android.hardware.usb.action.USB_DEVICE_DETACHED");
        child.Attributes.Append(newAttribute);
        newAttribute = CreateAndroidAttribute("resource", "@xml/device_filter");
        child.Attributes.Append(newAttribute);
    }
}